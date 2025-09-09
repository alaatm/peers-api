import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import common_en from './locales/en/common.json';
import common_ar from './locales/ar/common.json';
import forms_en from './locales/en/forms.json';
import forms_ar from './locales/ar/forms.json';
import components_en from './locales/en/components.json';
import components_ar from './locales/ar/components.json';
import enums_en from './locales/en/enums.json';
import enums_ar from './locales/ar/enums.json';
import { getStoredLangOrBrowserLang } from './utils';

i18n
    .use(initReactI18next)
    .init({
        resources: {
            en: {
                common: common_en,
                forms: forms_en,
                components: components_en,
                enums: enums_en,
            },
            ar: {
                common: common_ar,
                forms: forms_ar,
                components: components_ar,
                enums: enums_ar,
            },
        },
        lng: getStoredLangOrBrowserLang().code,
        fallbackLng: 'en',
        ns: ['common', 'forms', 'components', 'enums'],
        defaultNS: 'common',
        interpolation: { escapeValue: false },
        debug: import.meta.env.MODE === 'development',
    });

export default i18n;
