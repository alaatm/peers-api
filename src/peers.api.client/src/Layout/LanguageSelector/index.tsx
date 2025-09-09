import { Divider, Space } from 'antd';
import { supportedLangs, useLang } from '@/Language';
import './index.css';

const LanguageSelector = () => {
    const { lang: { code }, changeLang } = useLang();

    return (
        <Space className="language-selector" split={<Divider type="vertical" />}>
            {supportedLangs.map((l) => (
                <span key={l.code} className={code === l.code ? 'active' : undefined} onClick={() => changeLang(l)}>
                    {l.name}
                </span>
            ))}
        </Space>
    )
};

export default LanguageSelector;
