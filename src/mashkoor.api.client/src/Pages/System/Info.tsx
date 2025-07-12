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
                title={t('mashkoorApiInfo')}
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
                <Descriptions.Item label="version">{data.database.version}</Descriptions.Item>
                <Descriptions.Item label="serviceLevelObjective">{data.database.serviceLevelObjective}</Descriptions.Item>
                <Descriptions.Item label="dtuLimit">{data.database.dtuLimit}</Descriptions.Item>
                <Descriptions.Item label="cpuLimit">{data.database.cpuLimit}</Descriptions.Item>
                <Descriptions.Item label="minCores">{data.database.minCores}</Descriptions.Item>
                <Descriptions.Item label="maxDop">{data.database.maxDop}</Descriptions.Item>
                <Descriptions.Item label="maxSessions">{data.database.maxSessions}</Descriptions.Item>
                <Descriptions.Item label="maxDbMemory">{data.database.maxDbMemory}</Descriptions.Item>
                <Descriptions.Item label="maxDbMaxSizeInMb">{data.database.maxDbMaxSizeInMb}</Descriptions.Item>
                <Descriptions.Item label="checkpointRateIO">{data.database.checkpointRateIO}</Descriptions.Item>
                <Descriptions.Item label="checkpointRateMbps">{data.database.checkpointRateMbps}</Descriptions.Item>
                <Descriptions.Item label="primaryGroupMaxWorkers">{data.database.primaryGroupMaxWorkers}</Descriptions.Item>
                <Descriptions.Item label="primaryMaxLogRate">{data.database.primaryMaxLogRate}</Descriptions.Item>
                <Descriptions.Item label="primaryGroupMaxIO">{data.database.primaryGroupMaxIO}</Descriptions.Item>
                <Descriptions.Item label="primaryGroupMaxCpu">{data.database.primaryGroupMaxCpu}</Descriptions.Item>
                <Descriptions.Item label="volumeTypeManagedXstoreIOPS">{data.database.volumeTypeManagedXstoreIOPS}</Descriptions.Item>
            </Descriptions>
        </>) || null}
    </Card>;
}

export default SystemInfo;
