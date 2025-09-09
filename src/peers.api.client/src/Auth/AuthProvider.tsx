import { useCallback, useEffect, useState } from 'react';
import AuthContext from './AuthContext';
import { api } from '@/api';

function AuthProvider({ children }: { children: React.ReactNode }) {
    const [isAuthenticated, setIsAuthenticated] = useState(api.isAuthenticated);

    const signin = async (username: string, password: string, remeberMe: boolean) => {
        if (await api.login(username, password, remeberMe)) {
            setIsAuthenticated(true);
            return true;
        }

        return false;
    };

    const signout = useCallback(() => {
        api.logout();
        setIsAuthenticated(false);
    }, []);

    useEffect(() => {
        api.onUnauthorizedCallback = signout;
        return () => {
            api.onUnauthorizedCallback = null;
        };
    }, [signout]);

    const value = { isAuthenticated, jwtInfo: api.jwtInfo, signin, signout };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export default AuthProvider;
