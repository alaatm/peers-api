import { useState } from 'react';
import { Outlet } from 'react-router';
import { useTranslation } from 'react-i18next';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { theme, Layout, Row, Space, Badge, Divider } from 'antd';
import AppMenu from './AppMenu';
import UserMenu from './UserMenu';
import LanguageSelector from './LanguageSelector';
import BreadcrumbNav from './BreadcrumbNav';
import { useLang } from '@/Language';
import { api } from '@/api';

import logo from '@/assets/logo2.png';
import './index.css';
import { MenuToggle } from '@/components';

const { Header, Content, Sider, Footer } = Layout;

const MainLayout = () => {
    const { t } = useTranslation();
    const [collapsed, setCollapsed] = useState(false);
    const { lang: { dir } } = useLang();
    const siderWidth = collapsed ? 0 : 250;

    const {
        token: { colorBgContainer, borderRadiusLG },
    } = theme.useToken();

    return (
        <Layout className="main-layout" style={{ minHeight: '100vh' }}>
            <Header
                className="main-layout-header" style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    right: 0,
                    zIndex: 100,
                }}>
                <div className="logo">
                    <img src={logo} alt="logo" />
                    <span>{api.tenantName}</span>
                </div>
                <Space size="small" split={<Divider type="vertical" />}>
                    <LanguageSelector />
                    <Badge count={0} size="small">
                        <FontAwesomeIcon style={{ color: '#fff', fontSize: '1.2em' }} icon="bell" transform="down-2 left-1" />
                    </Badge>
                    <UserMenu />
                </Space>
            </Header>
            <Layout className="inner-layout">
                <Sider
                    width={250}
                    trigger={null}
                    collapsible
                    collapsedWidth="0"
                    collapsed={collapsed}
                    style={{
                        position: 'fixed',
                        top: '48px', // header height
                        bottom: 0,
                        [dir === 'ltr' ? 'left' : 'right']: 0,
                        background: colorBgContainer,
                        boxShadow: '0 25px 50px -12px rgba(0, 0, 0, .25)',
                        overflowY: 'auto',
                        overscrollBehavior: 'contain',
                    }}>
                    <AppMenu />
                </Sider>
                <Layout style={{
                    transition: 'margin 0.2s, background 0s',
                    [dir === 'ltr' ? 'marginLeft' : 'marginRight']: siderWidth,
                    marginTop: 48,
                    padding: '0 24px 24px'
                }}>
                    <Row style={{ alignItems: 'center' }}>
                        <Space>
                            <MenuToggle collapsed={collapsed} onToggle={setCollapsed} />
                            <BreadcrumbNav />
                        </Space>
                    </Row>
                    <Content
                        style={{
                            margin: 0,
                            minHeight: 280,
                            borderRadius: borderRadiusLG,
                        }}
                    >
                        <Outlet />
                    </Content>
                    <Footer style={{ paddingBottom: 0, textAlign: 'center' }}>
                        <Space>
                            <FontAwesomeIcon icon="copyright" />{new Date().getFullYear()}
                            <img src={logo} alt={t('appName')} width={15} />
                            {t('appName')}
                        </Space>
                    </Footer>
                </Layout>
            </Layout>
        </Layout>
    );
}

export default MainLayout;
