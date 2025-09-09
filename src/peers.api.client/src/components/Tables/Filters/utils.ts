import dayjs from 'dayjs';

export type LogicalOperation = 'and' | 'or';
export type FunctionName = 'startswith' | 'contains' | 'endswith' | 'lt' | 'le' | 'eq' | 'ge' | 'gt' | 'ne';

export type LogicalFilter = {
    type: 'logical';
    value: LogicalOperation;
}

export type FunctionFilter = |
{
    type: 'func';
    value: {
        func: FunctionName;
        arg: dayjs.Dayjs | undefined;
        dateOnly: boolean;
    };
} | {
    type: 'func';
    value: {
        func: FunctionName;
        arg: string | number | boolean | undefined;
    };
};

export type Filter = LogicalFilter | FunctionFilter;

// Examples:
// ['eq()'] =>
//                                               [{ type: 'func', value: { func: 'eq', arg: undefined } }]
// ['eq(true)'] =>
//                                               [{ type: 'func', value: { func: 'eq', arg: true } }]
// ['startswith(value)', 'or', 'eq(fixed)'] =>
//                                               [{ type: 'func', value: { func: 'startswith', arg: 'value' } },
//                                                { type: 'logical', value: 'or' },
//                                                { type: 'func', value: { func: 'eq', arg: 'fixed' } }]
// ['eq(1)', 'or', 'eq(2)'] =>
//                                               [{ type: 'func', value: { func: 'eq', arg: 1 } },
//                                                { type: 'logical', value: 'or' },
//                                                { type: 'func', value: { func: 'eq', arg: 2 } }]
// ['ge(2020-01-01)', 'and', 'le(2025-01-01)'] =>
//                                               [{ type: 'func', value: { func: 'ge', arg: dayjs('2020-01-01'), dateOnly: true } },
//                                                { type: 'logical', value: 'and' },
//                                                { type: 'func', value: { func: 'le', arg: dayjs('2025-01-01'), dateOnly: true } }]
// ['eq(2020-01-01T00:00:00.000Z)'] =>
//                                               [{ type: 'func', value: { func: 'eq', arg: dayjs('2020-01-01T00:00:00.000Z'), dateOnly: false } }]
export function deserializeFilters(keys: React.Key[]) {
    const funcRegex = /^(startswith|contains|endswith|lt|le|eq|ge|gt|ne)\(([^)]*)\)$/;
    const dateOnlyRegex = /^[0-9]{4}-[0-9]{2}-[0-9]{2}$/;

    return keys.map((key): Filter => {
        const matches = funcRegex.exec(key as string);

        if (matches) {
            const func = matches[1] as FunctionName;
            let arg: string | number | boolean | dayjs.Dayjs | undefined = matches[2];

            if (arg === '') {
                arg = undefined;
            } else if (arg.toLowerCase() === 'true' || arg.toLowerCase() === 'false') {
                arg = arg.toLowerCase() === 'true';
            } else if (!isNaN(Number(arg))) {
                arg = Number(arg);
            } else {
                const parsedDate = dayjs(arg);
                if (parsedDate.isValid()) {
                    return {
                        type: 'func',
                        value: { func, arg: parsedDate, dateOnly: dateOnlyRegex.test(arg) }
                    };
                }
            }

            return {
                type: 'func',
                value: { func, arg }
            };
        }

        if (key === 'and' || key === 'or') {
            return {
                type: 'logical',
                value: key
            };
        }

        throw new Error(`Invalid logical operator: ${key}`);
    });
}

// Opposite of deserializeFilters
export function serializeFilters(filters: Filter[]) {
    return filters.map((filter): string => {
        if (filter.type === 'logical') {
            return filter.value; // "and" or "or"
        }

        if (filter.type === 'func') {
            const { func, arg } = filter.value;

            let argString: string;
            if (arg === undefined) {
                argString = '';
            } else if (typeof arg === 'boolean') {
                argString = arg ? 'true' : 'false';
            } else if (dayjs.isDayjs(arg)) {
                const dateFilter = filter as Extract<FunctionFilter, { value: { arg: dayjs.Dayjs | undefined } }>;
                argString = dateFilter.value.dateOnly
                    ? arg.format('YYYY-MM-DD')
                    : arg.toISOString();
            } else {
                argString = String(arg);
            }

            return `${func}(${argString})`;
        }

        throw new Error(`Unexpected filter type: ${JSON.stringify(filter)}`);
    });
}

export function isValidFilter(filter: Filter): boolean {
    if (filter.type === 'func') {
        return filter.value.arg !== undefined && filter.value.arg !== '';
    }
    return true; // Logical filters are always valid
}

export function getInitialFilterState(selectedKeys: React.Key[], dateOnly?: boolean): Filter[] {
    if (selectedKeys.length === 0) {
        return dateOnly === undefined
            ? [
                { type: 'func', value: { func: 'eq', arg: undefined } }
            ]
            : [
                { type: 'func', value: { func: 'eq', arg: undefined, dateOnly } }
            ];
    }

    return deserializeFilters(selectedKeys);
}
