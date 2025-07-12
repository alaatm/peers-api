import { useTranslation } from 'react-i18next';
import { Card, Descriptions } from 'antd';
import { ColumnsType } from 'antd/es/table';
import { AjaxTable, BooleanFilter, DateFilter, OptionsFilter, TextFilter } from '@/components/Tables';
import { FormattedDate } from '@/components';
import { api, DeviceError } from '@/api';
import './index.css';

const CrashReports = () => {
    const { t } = useTranslation();

    const columns: ColumnsType<DeviceError> = [{
        key: 'reportedOn',
        dataIndex: 'reportedOn',
        title: t('date'),
        sorter: true,
        defaultSortOrder: 'descend',
        filterDropdown: (props) => <DateFilter {...props} dateOnly={false} />,
        render: (_, p) => <FormattedDate value={p.reportedOn} />,
    }, {
        key: 'username',
        dataIndex: 'username',
        title: t('username'),
        sorter: true,
        filterDropdown: (props) => <TextFilter {...props} />,
    }, {
        key: 'locale',
        dataIndex: 'locale',
        title: t('locale'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={[
            { label: 'Arabic', value: 'ar' },
            { label: 'English', value: 'en' }]} />,
    }, {
        key: 'silent',
        dataIndex: 'silent',
        title: t('silent'),
        sorter: true,
        filterDropdown: (props) => <BooleanFilter {...props} />,
        render: (_, p) => <span>{p.silent ? 'Yes' : 'No'}</span>,
    }, {
        key: 'source',
        dataIndex: 'source',
        title: t('source'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={[
            { label: t('flutter'), value: 'flutter' },
            { label: t('platform'), value: 'platform' }]} />,
    }, {
        key: 'appVersion',
        dataIndex: 'appVersion',
        title: t('appVersion'),
        sorter: true,
        filterDropdown: (props) => <TextFilter {...props} />,
    }, {
        key: 'appState',
        dataIndex: 'appState',
        title: t('appState'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={[
            { label: t('appStateOptions.resumed'), value: 'resumed' },
            { label: t('appStateOptions.inactive'), value: 'inactive' },
            { label: t('appStateOptions.paused'), value: 'paused' },
            { label: t('appStateOptions.detached'), value: 'detached' }]} />,
    }];

    return (
        <Card className="device-errors" title={t('deviceErrors')}>
            <AjaxTable<DeviceError>
                rowKey={record => record.id}
                columns={columns}
                fetchData={p => api.listCrashReports(p)}
                initialSort={{ field: 'reportedOn', order: 'descend' }}
                expandable={{
                    expandedRowRender: (record) => {
                        const deviceInfo = record.deviceInfo !== null ? JSON.parse(record.deviceInfo) : {};
                        const deviceInfoMap =
                            <Descriptions
                                bordered
                                size="small"
                                column={1}>
                                {Object.keys(deviceInfo).map(k =>
                                    <Descriptions.Item key={k} label={k}>{deviceInfo[k]}</Descriptions.Item>)}
                            </Descriptions>;
                        const descr =
                            <Descriptions
                                bordered
                                size="small"
                                column={1}>
                                <Descriptions.Item label="Device id">{record.deviceId}</Descriptions.Item>
                                <Descriptions.Item label="Device info">{deviceInfoMap}</Descriptions.Item>
                                <Descriptions.Item className="code-formatted" label="Info">{record.info.map((p, i) => <div key={`info-${i}`}>{p}</div>)}</Descriptions.Item>
                                <Descriptions.Item className="code-formatted" label="Exception">{record.exception}</Descriptions.Item>
                                <Descriptions.Item className="code-formatted" label="Stack trace">{record.stackTrace.map((p, i) => <div key={`trace-${i}`}>{p}</div>)}</Descriptions.Item>
                            </Descriptions>;

                        return descr;
                    },
                }}
            />
        </Card>
    );
};

export default CrashReports;
