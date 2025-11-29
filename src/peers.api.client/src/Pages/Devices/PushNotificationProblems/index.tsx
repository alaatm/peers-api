import { useTranslation } from 'react-i18next';
import { Card } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { api, FirebaseErrorCode, FirebaseMessagingErrorCode, type PushNotificationProblem } from '@/api';
import { AjaxTable, DateFilter, OptionsFilter, TextFilter } from '@/components/Tables';
import { FormattedDate } from '@/components';
import { enumToOptionsTrans } from '@/utils';


const PushNotificationProblems = () => {
    const { t } = useTranslation(['common', 'enums']);

    const columns: ColumnsType<PushNotificationProblem> = [{
        key: 'token',
        dataIndex: 'token',
        title: t('token'),
        sorter: true,
        filterDropdown: (props) => <TextFilter {...props} />,
        render: (p: string) => <span>{p}</span>,
    }, {
        key: 'reportedOn',
        dataIndex: 'reportedOn',
        title: t('date'),
        sorter: true,
        filterDropdown: (props) => <DateFilter {...props} dateOnly={false} />,
        render: (_, p) => <FormattedDate value={p.reportedOn} />,
    }, {
        key: 'errorCode',
        dataIndex: 'errorCode',
        title: t('errorCode'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={enumToOptionsTrans(t, FirebaseErrorCode)} />,
        render: (_, p) => FirebaseErrorCode[p.errorCode],
    }, {
        key: 'messagingErrorCode',
        dataIndex: 'messagingErrorCode',
        title: t('messagingErrorCode'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={enumToOptionsTrans(t, FirebaseMessagingErrorCode)} />,
        render: (_, p) => p.messagingErrorCode === null ? '-' : FirebaseMessagingErrorCode[p.messagingErrorCode],
    }];

    return (
        <Card title={t('pushNotificationProblems')}>
            <AjaxTable<PushNotificationProblem>
                rowKey={record => record.id}
                initialSort={{ field: 'reportedOn', order: 'descend' }}
                columns={columns}
                fetchData={p => api.listPushNotificationProblems(p)}
            />
        </Card>
    );
};

export default PushNotificationProblems;
