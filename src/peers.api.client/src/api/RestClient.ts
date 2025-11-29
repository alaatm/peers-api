import dayjs from 'dayjs';
import JwtInfo from './JwtInfo';
import TimeZone from './TimeZone';
import { type Args, type DateArg, GrantType, type HttpMethod, type JwtResponse, type PagedQueryResponse, type ProblemDetails } from './baseTypes';

class RestClient {
    #currentLang: string | null = null;
    jwtInfo = new JwtInfo();

    get username() { return this.jwtInfo.username; }
    get isAuthenticated() { return this.jwtInfo.isAuthenticated; }
    get requiresRefresh() { return this.jwtInfo.requiresRefresh; }
    get jwtExpiry() { return this.jwtInfo.tokenExpiry; }

    onUnauthorizedCallback: (() => void) | null = null;
    private unauthorized() {
        if (!this.onUnauthorizedCallback) {
            throw new Error('onUnauthorizedCallback is not set');
        }
        this.onUnauthorizedCallback();
    }

    async login(username: string, password: string, saveToken: boolean) {
        const payload = {
            createToken: {
                username,
                password,
                grantType: GrantType.Password,
                userType: 2 /* staff */,
            },
        };

        const response = await this.post<JwtResponse>('oauth/token', payload, undefined, false);
        if (isOk(response)) {
            this.jwtInfo.set(response, saveToken);
            return true;
        }

        return false;
    }

    logout() {
        this.jwtInfo.clear();
    }

    changePassword(currentPassword: string, newPassword: string, newPasswordConfirm: string) {
        return this.put(
            'account/password',
            {
                changePassword: {
                    currentPassword,
                    newPassword,
                    newPasswordConfirm,
                },
            },
        );
    }

    recoverPassword(username: string) {
        return this.put(
            'account/password',
            {
                resetPassword: {
                    username,
                },
            },
            undefined,
            false,
        );
    }

    resetPassword(username: string, otp: string, newPassword: string) {
        return this.put(
            'account/password',
            {
                resetPasswordConfirm: {
                    otp,
                    username,
                    newPassword,
                },
            },
            undefined,
            false,
        );
    }

    setLang(lang: string) {
        this.#currentLang = lang;
    }

    getPaged<T>(url: string, args?: Args, auth = true) {
        return this.send<PagedQueryResponse<T>>('GET', url, args, undefined, auth);
    }

    get<T>(url: string, args?: Args, auth = true) {
        return this.send<T>('GET', url, args, undefined, auth);
    }

    post<T>(url: string, payload: unknown, args?: Args, auth = true) {
        return this.send<T>('POST', url, args, payload, auth);
    }

    put<T>(url: string, payload: unknown, args?: Args, auth = true) {
        return this.send<T>('PUT', url, args, payload, auth);
    }

    patch<T>(url: string, payload: unknown, args?: Args, auth = true) {
        return this.send<T>('PATCH', url, args, payload, auth);
    }

    delete<T>(url: string, args?: Args, auth = true) {
        return this.send<T>('DELETE', url, args, undefined, auth);
    }

    async downloadFile(url: string, args?: Args, fileName?: string, auth = true): Promise<void | ProblemDetails> {
        const response = await this.sendCore('GET', url, args, undefined, auth);

        if (isError(response)) {
            return response;
        } else {
            const blob = await response.blob();

            // Check that the returned Blob is actually a binary file.
            if (blob.type.includes('json') || blob.type.includes('html')) {
                throw new Error(`Expected a binary file, but received content type: ${blob.type}`);
            }

            if (!fileName) {
                const contentDisposition = response.headers.get('Content-Disposition');
                if (contentDisposition) {
                    const matches = contentDisposition.match(/filename\*?=(?:UTF-8'')?["']?([^"';\n]+)["']?/i);
                    fileName = matches?.[1] || 'downloaded_file';
                } else {
                    fileName = 'downloaded_file';
                }
            }

            const blobUrl = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = blobUrl;
            a.download = fileName;
            a.style.display = 'none';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(blobUrl);
        }
    }

    private async send<T>(
        method: HttpMethod,
        url: string,
        args?: Args,
        payload?: unknown,
        auth = true): Promise<T | ProblemDetails> {
        const response = await this.sendCore(method, url, args, payload, auth);
        if (isError(response)) {
            return response;
        }

        const responseBody = await response.json().catch(() => ({}));
        return responseBody as T;
    }

    private async sendCore(
        method: HttpMethod,
        url: string,
        args?: Args,
        payload?: unknown,
        auth = true): Promise<Response | ProblemDetails> {
        if (auth && this.requiresRefresh) {
            if (!await this.tryRefreshToken()) {
                return {
                    type: 'unauthorized',
                    title: 'Unauthorized',
                    status: 401,
                    detail: 'User is not authorized',
                    instance: url,
                    ___isError: true,
                } as ProblemDetails;
            }
        }

        const fullUrl = this.buildUrl(url, args);
        const request = this.buildRequest(method, payload, auth);

        try {
            const response = await fetch(fullUrl, request);

            // If the response isn't OK, try to get error details as JSON
            if (!response.ok) {
                const responseBody = await response.json().catch(() => ({}));
                return {
                    ...responseBody,
                    type: responseBody.type || 'https://tools.ietf.org/html/rfc7807',
                    title: responseBody.title || 'An error occurred',
                    status: responseBody.status || response.status,
                    detail: responseBody.detail || response.statusText,
                    instance: responseBody.instance || fullUrl,
                    ___isError: true,
                } as ProblemDetails;
            }

            return response;
        } catch (error) {
            return {
                type: 'network-error',
                title: 'Network Error',
                status: 0,
                detail: (error as Error).message || 'A network error occurred',
                instance: fullUrl,
                ___isError: true,
            } as ProblemDetails;
        }
    }

    #refreshPromise: Promise<boolean> | null = null;
    private async tryRefreshToken() {
        // If a refresh is already in progress, return the existing promise.
        if (this.#refreshPromise) {
            return this.#refreshPromise;
        }

        this.#refreshPromise = (async () => {
            try {
                if (!this.jwtInfo.refreshToken) {
                    this.unauthorized();
                    return false;
                }

                if (this.isAuthenticated && this.requiresRefresh) {
                    const payload = {
                        createToken: {
                            username: this.jwtInfo.username!,
                            password: this.jwtInfo.refreshToken,
                            grantType: GrantType.RefreshToken,
                        },
                    };

                    const response = await this.post<JwtResponse>('oauth/token', payload, undefined, false);
                    if (isOk(response)) {
                        this.jwtInfo.set(response);
                        return true;
                    }

                    this.unauthorized();
                }

                return this.isAuthenticated;
            } finally {
                this.#refreshPromise = null;
            }
        })();

        return this.#refreshPromise;
    }

    private buildUrl(url: string, args?: Args) {
        const params = new URLSearchParams();

        for (const key in args ?? {}) {
            const value = args?.[key];
            if (value === undefined || value === null) {
                continue;
            }

            if (Array.isArray(value)) {
                value.forEach((v) => params.append(key, this.formatValue(v)));
            } else {
                params.append(key, this.formatValue(value));
            }
        }

        const queryString = params.size > 0 ? `?${params.toString()}` : '';
        const fullUrl = `${url}${queryString}`;

        return url.indexOf('_health/') >= 0 || url.indexOf('_nt/') >= 0
            ? `/${fullUrl}`.replace(/\/+/g, '/')
            : `/api/v1/${fullUrl}`.replace(/\/+/g, '/');
    }

    private formatValue(value: Exclude<NonNullable<Args[keyof Args]>, Extract<Args[keyof Args], unknown[]>>) {
        if (this.isDateArg(value)) {
            return value.type === 'DateOnly'
                ? value.value.format('YYYY-MM-DD')
                : value.value.toISOString();
        }

        return String(value);
    }

    private buildRequest(method: HttpMethod, payload?: unknown, auth = true): RequestInit {
        const headers: HeadersInit = {
            'Accept': 'application/json',
            'X-TIMEZONE': TimeZone,
        };

        let body: BodyInit | undefined;

        if (payload instanceof FormData) {
            // Use FormData as-is; let the browser set Content-Type.
            body = payload;
        } else if (payload instanceof Blob) {
            // Use Blob as-is; set the Content-Type if available.
            body = payload;
            if (payload.type) {
                headers['Content-Type'] = payload.type;
            }
        } else if (payload !== undefined) {
            // Otherwise, assume a JSON payload.
            headers['Content-Type'] = 'application/json';
            body = JSON.stringify(payload, this.jsonReplacer.bind(this));
        }

        if (this.#currentLang) {
            headers['Accept-Language'] = this.#currentLang;
        }
        if (auth) {
            headers['Authorization'] = `Bearer ${this.jwtInfo.token}`;
        }

        return {
            method,
            headers,
            body,
        };
    }

    private jsonReplacer(_key: string, value: unknown): unknown {
        if (this.isDateArg(value)) {
            return value.type === 'DateOnly'
                ? value.value.format('YYYY-MM-DD')
                : value.value.toISOString();
        }

        // Ensure that Dayjs objects are not sent as-is.
        if (dayjs.isDayjs(value)) {
            throw new Error('Dayjs objects must be wrapped in a DateArg object');
        }

        return value;
    }

    private isDateArg(value: unknown): value is DateArg {
        return (
            typeof value === 'object' &&
            value !== null &&
            'value' in value &&
            'type' in value &&
            dayjs.isDayjs(value.value) &&
            (value.type === 'DateTime' || value.type === 'DateOnly')
        );
    }
}

export default RestClient;

export function isError(response: unknown): response is ProblemDetails {
    return typeof response === 'object' &&
        response !== null &&
        '___isError' in response &&
        (response as ProblemDetails).___isError === true;
}

export function isOk<T>(response: T | ProblemDetails): response is T {
    return !isError(response);
}

export function isNotEmpty<T extends object>(response: T | ProblemDetails): response is T {
    return isOk(response) && (
        Array.isArray(response) ? response.length > 0 : Object.keys(response).length > 0
    );
}
