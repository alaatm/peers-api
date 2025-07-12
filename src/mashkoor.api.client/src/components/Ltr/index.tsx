type Props = {
    value: string;
};

const Ltr = ({ value }: Props) => <span className="force-ltr">{value}</span>;

export default Ltr;