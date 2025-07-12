import dayjs from 'dayjs';
import { PeriodKind } from '@/api';

export const initialMultiDateData = [dayjs().startOf('day')];

export const initialPeriodData = {
    kind: PeriodKind.Year,
    date: dayjs().startOf('year'),
    numPeriods: 1,
    customNumDays: null as number | null,
};

export type PeriodData = typeof initialPeriodData;

export function getPickerForKind(kind: PeriodKind) {
    switch (kind) {
        case PeriodKind.Year:
            return 'year';
        case PeriodKind.Quarter:
            return 'quarter';
        case PeriodKind.Month:
            return 'month';
        case PeriodKind.Week:
            return 'week';
        case PeriodKind.Day:
            return 'date';
        default:
            return 'date';
    }
};

export function isSameMultiDateData(a: (dayjs.Dayjs | undefined)[], b: (dayjs.Dayjs | undefined)[]) {
    return a.length === b.length &&
        a.every((date, index) => date?.isSame(b[index]));
}

export function isSamePeriodData(a: PeriodData, b: PeriodData) {
    return (
        a.kind === b.kind &&
        a.date.isSame(b.date) &&
        a.numPeriods === b.numPeriods &&
        a.customNumDays === b.customNumDays
    );
}

export function isInitialPeriodData(data: PeriodData) {
    return isSamePeriodData(data, initialPeriodData);
}