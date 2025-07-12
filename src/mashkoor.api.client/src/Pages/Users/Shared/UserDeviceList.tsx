import { useTranslation } from 'react-i18next';
import { Descriptions } from 'antd';
import ClientTasks from '@/Pages/Users/Shared/ClientTasks';
import { AjaxTable } from '@/components/Tables';
import { api, DeviceSummary } from '@/api';
import { ColumnsType } from 'antd/es/table';
import { FormattedDate } from '@/components';

type Props = {
    userId: number;
};

const UserDeviceList = ({ userId }: Props) => {
    const { t } = useTranslation();

    const columns: ColumnsType<DeviceSummary> = [{
        key: 'manufacturer',
        dataIndex: 'manufacturer',
        title: t('manufacturer'),
        sorter: true,
    }, {
        key: 'model',
        dataIndex: 'model',
        title: t('model'),
        sorter: true,
    }, {
        key: 'platform',
        dataIndex: 'platform',
        title: t('platform'),
        sorter: true,
    }, {
        key: 'osVersion',
        dataIndex: 'osVersion',
        title: t('osVersion'),
        sorter: true,
    }, {
        key: 'idiom',
        dataIndex: 'idiom',
        title: t('idiom'),
        sorter: true,
    }, {
        key: 'deviceType',
        dataIndex: 'deviceType',
        title: t('type'),
        sorter: true,
    }, {
        key: 'registeredOn',
        dataIndex: 'registeredOn',
        title: t('registeredOn'),
        sorter: true,
        render: (_, p) => <FormattedDate value={p.registeredOn} />,
    }, {
        key: 'appVersion',
        dataIndex: 'appVersion',
        title: t('appVersion'),
        sorter: true,
    }, {
        key: 'isStalled',
        dataIndex: 'isStalled',
        title: t('isStalled'),
        sorter: true,
        render: (_, p) => <span>{t(`yesNo.${p.isStalled}`)}</span>,
    }, {
        key: 'lastPing',
        dataIndex: 'lastPing',
        title: t('lastPing'),
        sorter: true,
        render: (_, p) => <FormattedDate value={p.lastPing} />,
    }, {
        key: 'faults',
        dataIndex: 'faults',
        title: t('faults'),
        sorter: true,
    }];

    return (
        <AjaxTable<DeviceSummary>
            rowKey={(record) => record.id}
            columns={columns}
            fetchData={p => api.listUserDevices(userId, p)}
            expandable={{
                expandedRowRender: (record) => <>
                    <Descriptions
                        bordered
                        styles={{ label: { width: '10em' } }}
                        size="small"
                        column={1}>
                        <Descriptions.Item label={t('deviceId')}>{record.deviceId}</Descriptions.Item>
                        <Descriptions.Item label={t('handle')}>{record.handle}</Descriptions.Item>
                    </Descriptions>
                    <ClientTasks deviceId={record.deviceId} />
                </>
            }}
        />
    );
}

export default UserDeviceList;
