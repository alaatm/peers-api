type Props = {
    className?: string;
    value?: number | null;
    monospaced?: boolean;
};

const FinancialValue = ({ className, value, monospaced = false }: Props) => {
    const v = value ?? 0;

    const formatted = v.toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
        signDisplay: 'never',
    });

    return v < 0
        ? <span className={className} style={{ fontFamily: monospaced ? 'monospace' : undefined }}>({formatted})</span>
        : <span className={className} style={{ fontFamily: monospaced ? 'monospace' : undefined }}>{formatted}</span>;
};

export default FinancialValue;
