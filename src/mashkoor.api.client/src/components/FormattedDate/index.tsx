import { Space } from 'antd';
import dayjs from 'dayjs';
import TimeAgo from 'javascript-time-ago';
import ar from 'javascript-time-ago/locale/ar';
import en from 'javascript-time-ago/locale/en';
import { useLang } from '@/Language';

TimeAgo.addLocale(ar);
TimeAgo.addLocale(en);

type Props = {
    className?: string;
    value: string | dayjs.Dayjs | null | undefined;
    format?: 'long' | 'short';
    includeTimeAgo?: boolean;
};

const arFormatter = new TimeAgo('ar-SA');
const enFormatter = new TimeAgo('en-US');

const enLongWithTimeFormat = 'MMM D, YYYY, HH:mm';
const enLongWithoutTimeFormat = 'MMM D, YYYY';
const enShortFormat = 'YYYY-MM-DD';

const arLongWithTimeFormat = 'D MMM YYYY، HH:mm';
const arLongWithoutTimeFormat = 'DD MMM YYYY';
const arShortFormat = 'YYYY-MM-DD';

function getDateFormat(format: 'long' | 'short', lang: string, hasTimeComponent: boolean) {
    if (lang === 'ar') {
        return format === 'long'
            ? hasTimeComponent
                ? arLongWithTimeFormat
                : arLongWithoutTimeFormat
            : arShortFormat;
    } else {
        return format === 'long'
            ? hasTimeComponent
                ? enLongWithTimeFormat
                : enLongWithoutTimeFormat
            : enShortFormat;
    }
}

const FormattedDate = ({ className, value, format = 'long', includeTimeAgo = false }: Props) => {
    const { lang: { code } } = useLang();
    const timeAgoFormatter = code === 'ar' ? arFormatter : enFormatter;

    if (!value) {
        return <span className={className}>-</span>;
    }

    const date = dayjs(value).locale(code);
    const formattedDate = date.format(getDateFormat(format, code, date.hour() !== 0 || date.minute() !== 0));

    return (
        <span className={className}>
            <Space size="large">
                {formattedDate}
                {includeTimeAgo && `(${timeAgoFormatter.format(date.toDate())})`}
            </Space>
        </span>
    );
};

export default FormattedDate;
