import { useContext } from 'react';
import LangContext from './LangContext';

function useLang() {
    return useContext(LangContext);
}

export default useLang;
