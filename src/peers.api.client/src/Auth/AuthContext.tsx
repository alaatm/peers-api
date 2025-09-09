import { createContext } from 'react';
import { JwtInfo } from '@/api';

export type AuthContextType = {
    isAuthenticated: boolean;
    jwtInfo: JwtInfo;
    signin: (username: string, password: string, remeberMe: boolean) => Promise<boolean>;
    signout: () => void;
};

const AuthContext = createContext<AuthContextType>(null!);

export default AuthContext;
