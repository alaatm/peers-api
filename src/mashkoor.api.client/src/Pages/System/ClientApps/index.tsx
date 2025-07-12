import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Card, Table } from 'antd';
import { api, ClientApp, isOk } from '@/api';
import { ColumnsType } from 'antd/es/table';

const ClientApps = () => {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState<ClientApp[]>();

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            const response = await api.getClientApp();
            if (isOk(response)) {
                setData([response]);
            }
            setLoading(false);
        };

        fetchData();
    }, []);

    const columns: ColumnsType<ClientApp> = [{
        title: t('package'),
        dataIndex: 'packageName',
        key: 'packageName',
    }, {
        title: t('version'),
        dataIndex: 'versionString',
        key: 'versionString',
    }, {
        title: t('hashString'),
        dataIndex: 'hashString',
        key: 'hashString',
    }, {
        title: t('androidStoreLink'),
        dataIndex: 'androidStoreLink',
        key: 'androidStoreLink',
        render: (_, p) => <a href={p.androidStoreLink} target="_blank" rel="noreferrer">Link</a>
    }, {
        title: t('iosStoreLink'),
        dataIndex: 'iosStoreLink',
        key: 'iosStoreLink',
        render: (_, p) => <a href={p.iosStoreLink} target="_blank" rel="noreferrer">Link</a>
    }];

    return (
        <Card
            title={t('clientApps')}>
            <Table<ClientApp>
                loading={loading}
                rowKey={record => record.packageName}
                pagination={false}
                columns={columns}
                dataSource={data}
            />
        </Card>
    );
}

export default ClientApps;
