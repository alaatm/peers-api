import { useState, useEffect, JSX } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router';
import { Card } from 'antd';
import { PageTitle, Ltr } from '@/components';
import { api, Customer, isOk } from '@/api';
import { ChangeUserStatus, UserStatusInfo, UserAppUsageHistory, UserDeviceList } from '@/Pages/Users/Shared';
import DetailsBasicInfo from './DetailsBasicInfo';

const tab0Key = 'basic-info';
const tab3Key = 'usageHistory';
const tab4Key = 'devices';

const CustomerDetails = () => {
    const { t } = useTranslation();
    const [tabKey, setTabKey] = useState(tab0Key);
    const [customer, setCustomer] = useState<Customer>();

    const [reloadToken, setReloadToken] = useState(false);
    const { id } = useParams();

    useEffect(() => {
        const fetchData = async (id: string) => {
            const response = await api.getCustomer(id);
            if (isOk(response)) {
                setCustomer(response);
            }
        };

        if (id) {
            fetchData(id);
        }
    }, [id, reloadToken]);

    const tabList = [
        { key: tab0Key, tab: t('basicInfo') },
        { key: tab3Key, tab: t('usageHistory') },
        { key: tab4Key, tab: t('devices') },
    ];
    const tabContents: Record<string, JSX.Element> = {};
    tabContents[tab0Key] = customer ? <DetailsBasicInfo customer={customer} /> : <div />;
    tabContents[tab3Key] = customer ? <UserAppUsageHistory userId={customer.id} /> : <div />;
    tabContents[tab4Key] = customer ? <UserDeviceList userId={customer.id} /> : <div />;

    return (
        <Card
            title={customer && <PageTitle
                tags={<UserStatusInfo userStatus={customer.status} />}
                title={customer.name}
                subTitle={<Ltr value={customer.username} />}
            />}
            loading={!customer}
            tabList={tabList}
            activeTabKey={tabKey}
            onTabChange={t => setTabKey(t)}
            extra={customer && <ChangeUserStatus
                userId={customer.id}
                status={customer.status}
                onStatusChange={() => setReloadToken(prev => !prev)}
            />}
        >
            {tabContents[tabKey]}
        </Card>
    )
};

export default CustomerDetails;
