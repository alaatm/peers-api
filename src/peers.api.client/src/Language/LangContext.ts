import { createContext } from 'react';
import { Locale } from 'antd/es/locale';
import { Lang } from '@/api';

type Props = {
    lang: Lang;
    locale: Locale | undefined;
    changeLang: (newLang: Lang) => void;
}

const LangContext = createContext<Props>(null!);

export default LangContext;
