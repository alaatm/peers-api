import { LoginPage, useAuth } from '@/Auth';
import { Navigate, useLocation, useRoutes } from 'react-router';
import MainLayout from '@/Layout';
import ChangePasswordPage from '@/Layout/UserMenu/ChangePasswordPage';
import {
    CrashReports, CustomerDetails, CustomerList, DevicePush, NotFound,
    PushNotificationProblems, SystemClientApps, SystemInfo,
} from '@/Pages';

function App() {
    const { isAuthenticated } = useAuth();
    const location = useLocation();

    const Index = () => (
        <>
            <h1>TBD</h1>
        </>
    );

    const routes = [
        {
            path: '/',
            element: isAuthenticated ? <MainLayout /> : <Navigate to="/login" state={{ from: location }} />,
            children: [
                { index: true, element: <Index /> },
                { path: '/change-password', element: <ChangePasswordPage /> },

                { path: '/users/customers', element: <CustomerList /> },
                { path: '/users/customers/:id/', element: <CustomerDetails /> }, // With optional name param
                { path: '/users/customers/:id/:name', element: <CustomerDetails /> },

                { path: '/devices/errors', element: <CrashReports /> },
                { path: '/devices/push-notification-problems', element: <PushNotificationProblems /> },
                { path: '/devices/push', element: <DevicePush /> },

                { path: '/system/info', element: <SystemInfo /> },
                { path: '/system/client-apps', element: <SystemClientApps /> },

                { path: '*', element: <NotFound /> }
            ]
        },
        { path: 'login', element: <LoginPage /> },
    ];

    return useRoutes(routes);
}

export default App
