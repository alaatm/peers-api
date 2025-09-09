import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Card, Descriptions, Divider } from 'antd';
import { api, isOk, SysInfo } from '@/api';
import { FormattedDate } from '@/components';

const SystemInfo = () => {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState<SysInfo>();

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            const response = await api.getSystemInfo();
            if (isOk(response)) {
                setData(response);
            }
            setLoading(false);
        };

        fetchData();
    }, []);


    return <Card
        title={t('systemInfo')}
        loading={loading}>
        {(data && <>
            {/* App Info*/}
            <Descriptions
                title={t('peersApiInfo')}
                column={1}
                styles={{ label: { width: '25%' } }}
                bordered={true}>
                <Descriptions.Item label={t('version')}>{data.app.version}</Descriptions.Item>
                <Descriptions.Item label={t('buildDate')}><FormattedDate value={data.app.buildDate} /></Descriptions.Item>
            </Descriptions>
            <Divider />
            {/* Server Info*/}
            <Descriptions
                title={t('appServerInfo')}
                column={1}
                styles={{ label: { width: '25%' } }}
                bordered={true}>
                <Descriptions.Item label={t('osVersion')}>{data.server.osVersion}</Descriptions.Item>
                <Descriptions.Item label={t('runtimeVersion')}>{data.server.runtimeVersion}</Descriptions.Item>
            </Descriptions>
            <Divider />
            {/* Database Info*/}
            <Descriptions
                title={t('databaseServerInfo')}
                column={1}
                styles={{ label: { width: '25%' } }}
                bordered={true}>
                <Descriptions.Item label={t('version')}>{data.database.version}</Descriptions.Item>
                <Descriptions.Item label={t('dbSize')}>{data.database.dbSize}</Descriptions.Item>
                <Descriptions.Item label={t('dbUnallocatedSize')}>{data.database.unallocated}</Descriptions.Item>
                <Descriptions.Item label={t('dbReservedSize')}>{data.database.reserved}</Descriptions.Item>
                <Descriptions.Item label={t('dbDataSize')}>{data.database.data}</Descriptions.Item>
                <Descriptions.Item label={t('dbIndexSize')}>{data.database.indexSize}</Descriptions.Item>
                <Descriptions.Item label={t('dbUnusedSize')}>{data.database.unused}</Descriptions.Item>
            </Descriptions>
        </>) || null}
    </Card>;
}

export default SystemInfo;
