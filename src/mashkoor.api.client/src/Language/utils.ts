import { Lang } from '@/api';

const appName = import.meta.env.VITE_APP_NAME!;
const langKey = `ijaza.${appName}.lang`;

export const supportedLangs: Lang[] = [
    { code: 'en', dir: 'ltr', name: 'English' },
    { code: 'ar', dir: 'rtl', name: 'العربية' },
];

export function getStoredLangOrBrowserLang() {
    const lang = localStorage.getItem(langKey) || navigator.language.split('-')[0];
    return supportedLangs.find((l) => l.code === lang) || supportedLangs[0];
}

export function setStoredLang(lang: string) {
    localStorage.setItem(langKey, lang);
}
