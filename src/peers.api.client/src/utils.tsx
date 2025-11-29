import dayjs from 'dayjs';
import type { TFunction } from 'i18next';
import type { DateArg } from '@/api';

export function humanize(str: string) {
    return str.replace(/([A-Z])/g, ' $1').replace(/^./, (str) => str.toUpperCase());
}

export function extractEnumKeys<T extends object>(enumObj: T): (keyof T)[] {
    return Object
        .keys(enumObj)
        .filter(key => isNaN(Number(key)) && key !== 'translationKey') as (keyof T)[];
}

export function extractEnumKeysTrans<T extends Record<string, string | number>>(
    t: TFunction,
    enumObj: T,
): { key: string; label: string }[] {
    return Object.keys(enumObj)
        // Exclude numeric keys and any translation key helper property.
        .filter(key => isNaN(Number(key)) && key !== 'translationKey')
        .map(key => {
            const value = enumObj[key];
            return {
                key,
                label: te(t, enumObj, value as number)
            };
        });
}

export function enumToOptions<T extends Record<string, string | number>>(enumObj: T): { label: string; value: number }[] {
    return Object.entries(enumObj)
        .filter(([, value]) => typeof value === 'number') // Only keep numeric values (filter out reverse mappings)
        .map(([key, value]) => ({
            label: humanize(key), // Enum name (string)
            value: value as number // Enum numeric value
        }));
}

export function enumToOptionsTrans<T extends Record<string, string | number>>(
    t: TFunction,
    enumObj: T,
    excludedValues: number[] = [],
): { label: string; value: number }[] {
    return Object.entries(enumObj)
        .filter(([, value]) => typeof value === 'number') // Only keep numeric (forward mappings)
        .filter(([, value]) => !excludedValues.includes(value as number)) // Exclude specified values
        .map(([, value]) => ({
            label: te(t, enumObj, value as number),
            value: value as number
        }));
}

export function te<T extends Record<string, string | number>>(
    t: TFunction,
    enumObject: T,
    value: number
): string {
    const key = enumObject[value];
    return t(`enums:${enumObject.translationKey || 'UnknownEnum'}.${key}`);
}

type Normalized<T> =
    T extends dayjs.Dayjs ? DateArg :
    T extends Array<infer U> ? Normalized<U>[] :
    T extends object ? { [K in keyof T]: Normalized<T[K]> } :
    T;

/**
 * Recursively normalizes a payload by converting any Dayjs object
 * into a DateArg using the provided dateType.
 *
 * @param payload The payload to normalize.
 * @param dateType The type to assign to any Dayjs instance ('DateOnly' or 'DateTime').
 * @returns The normalized payload.
 */
export function wrapDayjsDates<T>(
    payload: T,
    dateType: 'DateTime' | 'DateOnly' = 'DateOnly'
): Normalized<T> {
    // If payload is null or undefined, return it as-is.
    if (payload === null || payload === undefined) {
        return payload as Normalized<T>;
    }

    // If payload is a Dayjs instance, wrap it in a DateArg.
    if (dayjs.isDayjs(payload)) {
        return { value: payload, type: dateType } as Normalized<T>;
    }

    // If it's an array, normalize each element.
    if (Array.isArray(payload)) {
        return payload.map(item => wrapDayjsDates(item, dateType)) as Normalized<T>;
    }

    // If it's an object, iterate over its keys.
    if (typeof payload === 'object') {
        const normalized: Record<string, unknown> = {};
        for (const key in payload) {
            if (Object.prototype.hasOwnProperty.call(payload, key)) {
                normalized[key] = wrapDayjsDates((payload as Record<string, unknown>)[key], dateType);
            }
        }
        return normalized as Normalized<T>;
    }

    // For primitives (string, number, boolean, etc.), return as-is.
    return payload as Normalized<T>;
}

export function assert(condition: unknown, message?: string): asserts condition {
    if (!condition) {
        throw new Error(message || 'Assertion failed');
    }
}
