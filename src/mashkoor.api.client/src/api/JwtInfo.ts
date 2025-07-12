import { JwtResponse } from './baseTypes';

export default class JwtInfo {
    #appName = import.meta.env.VITE_APP_NAME!;

    #usernameKey = `ijaza.${this.#appName}.username`;
    #tokenKey = `ijaza.${this.#appName}.token`;
    #refreshTokenKey = `ijaza.${this.#appName}.refreshToken`;
    #tokenExpiryKey = `ijaza.${this.#appName}.tokenExpiry`;

    get username() { return localStorage.getItem(this.#usernameKey); }
    get token() { return localStorage.getItem(this.#tokenKey); }
    get refreshToken() { return localStorage.getItem(this.#refreshTokenKey); }
    get tokenExpiry() {
        const expiry = localStorage.getItem(this.#tokenExpiryKey);
        return expiry ? Date.parse(expiry) : null;
    }

    get isAuthenticated() { return this.username != null; }
    get requiresRefresh() { return (this.tokenExpiry || 0) < Date.now(); }

    set(jwtResponse: JwtResponse, saveRefreshToken = true) {
        const { username, token, refreshToken, tokenExpiry } = jwtResponse;

        localStorage.setItem(this.#usernameKey, username);
        localStorage.setItem(this.#tokenKey, token);
        localStorage.setItem(this.#tokenExpiryKey, tokenExpiry);

        if (saveRefreshToken) {
            localStorage.setItem(this.#refreshTokenKey, refreshToken);
        }
    }

    clear() {
        localStorage.removeItem(this.#usernameKey);
        localStorage.removeItem(this.#tokenKey);
        localStorage.removeItem(this.#refreshTokenKey);
        localStorage.removeItem(this.#tokenExpiryKey);
    }
}
