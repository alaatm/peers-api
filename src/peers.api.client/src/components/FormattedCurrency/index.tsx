type Props = {
    className?: string;
    value: number;
    currency?: string;
    emptyIfZero?: boolean;
    monospaced?: boolean;
    pad?: number;
};

const FormattedCurrency = ({ className, value, currency, emptyIfZero = true, monospaced = false, pad = 0 }: Props) => {
    if (emptyIfZero && value === 0) {
        return <span className={className}>-</span>;
    }

    let formatted = value.toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    });

    if (currency) {
        formatted = `${formatted} ${currency}`;
    }
    if (pad) {
        formatted = formatted.padStart(pad, '\u00a0');
    }

    return <span className={className} style={{ fontFamily: monospaced ? 'monospace' : undefined }}>{formatted}</span>;
};

export default FormattedCurrency;