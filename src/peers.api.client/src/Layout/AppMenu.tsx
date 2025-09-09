import { useState } from 'react';
import { Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import { Menu, MenuProps } from 'antd';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

type MenuItem = Required<MenuProps>['items'][number];

const AppMenu = () => {
    const { t } = useTranslation();
    let path = window.location.pathname;

    if (path.charAt(0) === '/') {
        path = path.slice(1);
    }
    if (path.charAt(path.length - 1) === '/') {
        path = path.slice(0, path.length - 1);
    }

    let index = path.lastIndexOf('/');
    const selectedKeys = [path];

    while (index >= 0) {
        path = path.slice(0, index);
        selectedKeys.push(path);
        index = path.lastIndexOf('/');
    }

    const [openKeys, setOpenKeys] = useState(selectedKeys);
    const onOpenChange = (openKeys: string[]) => setOpenKeys(openKeys);

    const items: MenuItem[] = [
        {
            key: 'sub1',
            icon: <FontAwesomeIcon icon="gauge" />,
            label: t('dashboard'),
        },
        {
            key: 'users',
            icon: <FontAwesomeIcon icon="users" />,
            label: t('users'),
            children: [
                {
                    key: 'users/customers',
                    icon: <FontAwesomeIcon icon="user-group" />,
                    label: <Link to="/users/customers">{t('customers')}</Link>,
                },
                {
                    key: 'users/drivers',
                    icon: <FontAwesomeIcon icon="people-carry-box" />,
                    label: <Link to="/users/drivers">{t('drivers')}</Link>,
                },
                {
                    key: 'users/partners',
                    icon: <FontAwesomeIcon icon="handshake" />,
                    label: <Link to="/users/partners">{t('partners')}</Link>,
                },
                {
                    key: 'users/ratings',
                    icon: <FontAwesomeIcon icon="star-half-stroke" />,
                    label: <Link to="/users/ratings">{t('ratings')}</Link>,
                },
            ],
        },
        {
            key: 'bookings',
            icon: <FontAwesomeIcon icon="ticket" />,
            label: t('bookings'),
            children: [
                {
                    key: 'bookings/live-tracking',
                    icon: <FontAwesomeIcon icon="location-dot" />,
                    label: <Link to="/bookings/live-tracking">{t('liveTracking')}</Link>,
                },
                {
                    key: 'bookings/scheduled',
                    icon: <FontAwesomeIcon icon="hourglass-half" />,
                    label: <Link to="/bookings/scheduled">{t('scheduled')}</Link>,
                },
                {
                    key: 'bookings/open',
                    icon: <FontAwesomeIcon icon="check" />,
                    label: <Link to="/bookings/open">{t('open')}</Link>,
                },
                {
                    key: 'bookings/disputed',
                    icon: <FontAwesomeIcon icon="triangle-exclamation" />,
                    label: <Link to="/bookings/disputed">{t('disputed')}</Link>,
                },
                {
                    key: 'bookings/complete',
                    icon: <FontAwesomeIcon icon="check-double" />,
                    label: <Link to="/bookings/complete">{t('completed')}</Link>,
                },
                {
                    key: 'bookings/cancelled',
                    icon: <FontAwesomeIcon icon="ban" />,
                    label: <Link to="/bookings/cancelled">{t('cancelled')}</Link>,
                },
                {
                    key: 'bookings/expired',
                    icon: <FontAwesomeIcon icon="hourglass" />,
                    label: <Link to="/bookings/expired">{t('expired')}</Link>,
                },
                {
                    key: 'bookings/active-trips',
                    icon: <FontAwesomeIcon icon="car-on" />,
                    label: <Link to="/bookings/active-trips">{t('activeTrips')}</Link>,
                },
            ],
        },
        {
            key: 'approvals',
            icon: (
                <span className="fa-layers fa-fw fa-lg">
                    <FontAwesomeIcon icon="file" />
                    <FontAwesomeIcon icon="question-circle" transform="shrink-5 down-3 right-4" inverse />
                </span>
            ),
            label: t('approvals'),
            children: [
                {
                    key: 'approvals/identification',
                    icon: <FontAwesomeIcon icon="fingerprint" />,
                    label: <Link to="/approvals/identification">{t('identification')}</Link>,
                },
                {
                    key: 'approvals/driver-license',
                    icon: <FontAwesomeIcon icon="id-card" />,
                    label: <Link to="/approvals/driver-license">{t('driverLicense')}</Link>,
                },
                {
                    key: 'approvals/vehicle',
                    icon: <FontAwesomeIcon icon="car" />,
                    label: <Link to="/approvals/vehicle">{t('vehicle')}</Link>,
                },
            ],
        },
        {
            key: 'device',
            icon: <FontAwesomeIcon icon="mobile-screen" />,
            label: t('devices'),
            children: [
                {
                    key: 'devices/errors',
                    icon: <FontAwesomeIcon icon="explosion" />,
                    label: <Link to="/devices/errors">{t('errors')}</Link>,
                },
                {
                    key: 'devices/push-notification-problems',
                    icon: <FontAwesomeIcon icon="triangle-exclamation" />,
                    label: <Link to="/devices/push-notification-problems">{t('failedNotifications')}</Link>,
                },
                {
                    key: 'devices/push',
                    icon: <FontAwesomeIcon icon="envelope" />,
                    label: <Link to="/devices/push">{t('push')}</Link>,
                },
            ],
        },
        {
            key: 'finance',
            icon: <FontAwesomeIcon icon="hand-holding-dollar" />,
            label: t('finance'),
            children: [
                {
                    key: 'finance/transactions',
                    icon: <FontAwesomeIcon icon="money-bill-transfer" />,
                    label: t('transactions'),
                    children: [
                        {
                            key: 'finance/transactions/invoices',
                            icon: <FontAwesomeIcon icon="receipt" />,
                            label: <Link to="/finance/transactions/invoices">{t('invoices')}</Link>,
                        },
                        {
                            key: 'finance/transactions/undeposited-funds',
                            icon: <FontAwesomeIcon icon="credit-card" />,
                            label: <Link to="/finance/transactions/undeposited-funds">{t('undepositedFunds')}</Link>,
                        },
                        {
                            key: 'finance/transactions/payouts',
                            icon: <FontAwesomeIcon icon="money-check-dollar" />,
                            label: <Link to="/finance/transactions/payouts">{t('payouts')}</Link>,
                        },
                        {
                            key: 'finance/transactions/funding',
                            icon: <FontAwesomeIcon icon="sack-dollar" />,
                            label: <Link to="/finance/transactions/funding">{t('funding')}</Link>,
                        },
                    ],
                },
                {
                    key: 'finance/financial-statements',
                    icon: <FontAwesomeIcon icon="book" />,
                    label: t('financialStatements'),
                    children: [
                        {
                            key: 'finance/financial-statements/general-ledger',
                            icon: <FontAwesomeIcon icon="book" />,
                            label: <Link to="/finance/financial-statements/general-ledger">{t('generalLedger')}</Link>,
                        },
                        {
                            key: 'finance/financial-statements/balance-sheet',
                            icon: <FontAwesomeIcon icon="book" />,
                            label: <Link to="/finance/financial-statements/balance-sheet">{t('balanceSheet')}</Link>,
                        },
                        {
                            key: 'finance/financial-statements/cashflow-statement',
                            icon: <FontAwesomeIcon icon="book" />,
                            label: <Link to="/finance/financial-statements/cashflow-statement">{t('cashflowStatement')}</Link>,
                        },
                        {
                            key: 'finance/financial-statements/income-statement',
                            icon: <FontAwesomeIcon icon="book" />,
                            label: <Link to="/finance/financial-statements/income-statement">{t('incomeStatement')}</Link>,
                        },
                        {
                            key: 'finance/financial-statements/trial-balance',
                            icon: <FontAwesomeIcon icon="book" />,
                            label: <Link to="/finance/financial-statements/trial-balance">{t('trialBalance')}</Link>,
                        },
                    ],
                },
                {
                    key: 'finance/partners-balances',
                    icon: <FontAwesomeIcon icon="scale-balanced" />,
                    label: <Link to="/finance/partners-balances">{t('partnersBalances')}</Link>,
                },
            ],
        },
        {
            key: 'settings',
            icon: <FontAwesomeIcon icon="cogs" />,
            label: t('settings'),
            children: [
                {
                    key: 'settings/banks',
                    icon: <FontAwesomeIcon icon="bank" />,
                    label: <Link to="/settings/banks">{t('banks')}</Link>,
                },
                {
                    key: 'settings/locations',
                    icon: <FontAwesomeIcon icon="map-location-dot" />,
                    label: <Link to="/settings/locations">{t('locations')}</Link>,
                },
                {
                    key: 'settings/auto-makers',
                    icon: <FontAwesomeIcon icon="car" />,
                    label: <Link to="/settings/auto-makers">{t('autoMakers')}</Link>,
                },
                {
                    key: 'settings/categories',
                    icon: <FontAwesomeIcon icon="list-check" />,
                    label: t('categories'),
                    children: [
                        {
                            key: 'settings/categories/services',
                            icon: <FontAwesomeIcon icon="list-check" />,
                            label: <Link to="/settings/categories/services">{t('services')}</Link>,
                        },
                        {
                            key: 'settings/categories/vehicles',
                            icon: <FontAwesomeIcon icon="truck" />,
                            label: <Link to="/settings/categories/vehicles">{t('vehicles')}</Link>,
                        },
                    ],
                },
                {
                    key: 'settings/cancellations',
                    icon: <FontAwesomeIcon icon="rectangle-xmark" />,
                    label: t('cancellations'),
                    children: [
                        {
                            key: 'settings/cancellations/policies',
                            icon: <FontAwesomeIcon icon="right-left" />,
                            label: <Link to="/settings/cancellations/policies">{t('policies')}</Link>,
                        },
                        {
                            key: 'settings/cancellations/reasons',
                            icon: <FontAwesomeIcon icon="circle-question" />,
                            label: <Link to="/settings/cancellations/reasons">{t('reasons')}</Link>,
                        },
                    ],
                },
                {
                    key: 'settings/approvals',
                    icon: (
                        <span className="fa-layers fa-fw fa-lg">
                            <FontAwesomeIcon icon="file" />
                            <FontAwesomeIcon icon="question-circle" transform="shrink-5 down-3 right-4" inverse />
                        </span>
                    ),
                    label: t('approvals'),
                    children: [
                        {
                            key: 'settings/approvals/rejection-reasons',
                            icon: <FontAwesomeIcon icon="circle-question" />,
                            label: <Link to="/settings/approvals/rejection-reasons">{t('rejectionReasons')}</Link>,
                        },
                    ],
                },
                {
                    key: 'settings/profit-methods',
                    icon: <FontAwesomeIcon icon="calculator" />,
                    label: <Link to="/settings/profit-methods">{t('profitMethods')}</Link>,
                },
                {
                    key: 'settings/bookings',
                    icon: <FontAwesomeIcon icon="cogs" />,
                    label: <Link to="/settings/bookings">{t('bookings')}</Link>,
                },
                {
                    key: 'settings/payouts',
                    icon: <FontAwesomeIcon icon="money-bill-transfer" />,
                    label: <Link to="/settings/payouts">{t('payouts')}</Link>,
                },
                {
                    key: 'settings/front-end',
                    icon: <FontAwesomeIcon icon="mobile" />,
                    label: <Link to="/settings/front-end">{t('frontEnd')}</Link>,
                },
                {
                    key: 'settings/staff-access',
                    icon: <FontAwesomeIcon icon="user-lock" />,
                    label: <Link to="/settings/staff-access">{t('staffAccess')}</Link>,
                },
            ],
        },
        {
            key: 'system',
            icon: <FontAwesomeIcon icon="screwdriver-wrench" />,
            label: t('system'),
            children: [
                {
                    key: 'system/info',
                    icon: <FontAwesomeIcon icon="info" />,
                    label: <Link to="/system/info">{t('info')}</Link>,
                },
                {
                    key: 'system/health',
                    icon: <FontAwesomeIcon icon="heart-pulse" />,
                    label: <Link to="/system/health">{t('health')}</Link>,
                },
                {
                    key: 'system/rate-limits',
                    icon: <FontAwesomeIcon icon="scissors" />,
                    label: <Link to="/system/rate-limits">{t('rateLimits')}</Link>,
                },
                {
                    key: 'system/client-apps',
                    icon: <FontAwesomeIcon icon="mobile-screen-button" />,
                    label: <Link to="/system/client-apps">{t('clientApps')}</Link>,
                },
            ],
        },
        {
            key: 'notifications',
            icon: <FontAwesomeIcon icon="bell" />,
            label: <Link to="/notifications">{t('notifications')}</Link>,
        },
    ];

    return (
        <Menu
            mode="inline"
            defaultSelectedKeys={['1']}
            selectedKeys={selectedKeys}
            openKeys={openKeys}
            style={{ height: '100%', borderRight: 0 }}
            onOpenChange={onOpenChange}
            items={items} />
    );
};

export default AppMenu;
