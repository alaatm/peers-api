import dayjs from 'dayjs';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { AuthProvider } from '@/Auth';
import { LangProvider } from '@/Language';
import ConfigWrapper from './ConfigWrapper.tsx';
import quarterOfYear from 'dayjs/plugin/quarterOfYear';
import 'dayjs/locale/ar';
import './icons.ts';
import './index.css';

dayjs.extend(quarterOfYear);

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <AuthProvider>
            <LangProvider>
                <ConfigWrapper />
            </LangProvider>
        </AuthProvider>
    </StrictMode>,
)
