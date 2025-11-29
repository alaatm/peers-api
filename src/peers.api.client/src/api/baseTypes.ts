import dayjs from 'dayjs';
import type { Key } from 'react';

export const enum GrantType {
    Password = 1,
    RefreshToken = 2,
};

export type JwtResponse = {
    username: string;
    token: string;
    refreshToken: string;
    tokenExpiry: string;
    roles: string[];
};

export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    errors?: string[] | Record<string, string | string[]>;
    ___isError: true;
}

export type DateArg = { value: dayjs.Dayjs, type: 'DateTime' | 'DateOnly' }

export type Args = Record<
    string,
    | Key
    | string
    | number
    | boolean
    | DateArg
    | (string | number | boolean | DateArg)[]
    | undefined
    | null>;

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export type PagedQueryResponse<T> = {
    data: T[];
    total: number;
};
