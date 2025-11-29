import { useCallback, useEffect, useState } from 'react';
import type { Locale } from 'antd/es/locale';
import arEG from 'antd/locale/ar_EG';
import i18n from './i18n';
import LangContext from './LangContext';
import { api, type Lang } from '@/api';
import { getStoredLangOrBrowserLang, setStoredLang } from './utils';

const LangProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [lang, setLang] = useState<Lang>(getStoredLangOrBrowserLang());
    const [locale, setLocale] = useState<Locale | undefined>();

    useEffect(() => {
        const { code, dir } = lang;
        document.documentElement.lang = code;
        document.documentElement.dir = dir as string;
    }, [lang]);

    const changeLang = useCallback((newLang: Lang) => {
        const { code } = newLang;

        i18n.changeLanguage(code);
        api.setLang(code);
        setStoredLang(code);
        setLang(newLang);
        setLocale(code === 'ar' ? arEG : undefined);
    }, []);

    return (
        <LangContext.Provider value={{ lang, locale, changeLang }}>
            {children}
        </LangContext.Provider>
    );
}

export default LangProvider;
