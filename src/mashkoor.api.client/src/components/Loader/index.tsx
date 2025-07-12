import { Spin } from 'antd';
import { useTranslation } from 'react-i18next';
import './index.css';

type Props = {
    visible: boolean;
    text?: string;
    size?: 'large' | 'small' | 'default';
}

const Loader = ({ visible, text, size = 'default' }: Props) => {
    const { t } = useTranslation('components');

    return visible ? (
        <div className="loader">
            <Spin size={size} />
            <h1>{text ?? t('loading')}</h1>
        </div>
    ) : null;
};

export default Loader;
