import { ConfigProvider, theme } from 'antd';
import { BrowserRouter } from 'react-router';
import { useLang } from '@/Language';
import App from './App';

const ConfigWrapper = () => {
    const { lang, locale } = useLang();
    // Only override the font for Arabic; otherwise, use Ant Design’s default.
    const themeToken = lang.code === 'ar' ? { fontFamily: 'rubik' } : {};

    return (
        <ConfigProvider
            theme={{
                token: themeToken,
                algorithm: theme.defaultAlgorithm,
            }}
            direction={lang.dir}
            locale={locale}
        >
            <BrowserRouter>
                <App />
            </BrowserRouter>
        </ConfigProvider>
    );
};

export default ConfigWrapper;
