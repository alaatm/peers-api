import { useNavigate } from 'react-router';
import { useTranslation } from 'react-i18next';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Dropdown, MenuProps, Space } from 'antd';
import { useAuth } from '@/Auth';

const UserMenu = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { signout, jwtInfo } = useAuth();

    const items: MenuProps['items'] = [
        {
            key: 'change-password',
            icon: <FontAwesomeIcon icon="key" />,
            label: t('changePassword'),
            onClick: () => navigate('/change-password'),
        },
        {
            key: 'logout',
            label: <span>{t('signOut')}</span>,
            icon: <FontAwesomeIcon icon="power-off" />,
            onClick: () => signout(),
        },
    ];

    return (
        <Dropdown menu={{ items }} trigger={['click']}>
            <a onClick={(e) => e.preventDefault()}>
                <Space style={{ color: 'white' }}>
                    <FontAwesomeIcon icon="user" />
                    <span>{jwtInfo.username}</span>
                </Space>
            </a>
        </Dropdown>
    );
};

export default UserMenu;
