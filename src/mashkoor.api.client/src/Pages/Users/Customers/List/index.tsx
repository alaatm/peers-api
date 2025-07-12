import { Card } from 'antd';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import { ColumnsType } from 'antd/es/table';
import { AjaxTable, DateFilter, NumericFilter, OptionsFilter, TextFilter } from '@/components/Tables';
import { api, CustomerSummary, UserStatus } from '@/api';
import { enumToOptionsTrans } from '@/utils';
import { FormattedDate, Ltr } from '@/components';
import { UserStatusInfo } from '@/Pages/Users/Shared';

const CustomerList = () => {
    const { t } = useTranslation(['common', 'enums']);

    const columns: ColumnsType<CustomerSummary> = [{
        key: 'username',
        dataIndex: 'username',
        title: t('username'),
        sorter: true,
        filterDropdown: (props) => <TextFilter {...props} />,
        render: (_, p) => <Link to={`/users/customers/${p.id}/${p.username}`}><Ltr value={p.username} /></Link>,
    }, {
        key: 'name',
        dataIndex: 'name',
        title: t('name'),
        sorter: true,
        filterDropdown: (props) => <TextFilter {...props} />,
    }, {
        key: 'status',
        dataIndex: 'status',
        title: t('status'),
        sorter: true,
        filterDropdown: (props) => <OptionsFilter {...props} options={enumToOptionsTrans(t, UserStatus)} />,
        render: (_, p) => <UserStatusInfo userStatus={p.status} />,
    }, {
        key: 'bookingsCount',
        dataIndex: 'bookingsCount',
        title: t('bookingsCount'),
        sorter: true,
        filterDropdown: (props) => <NumericFilter {...props} />,
    }, {
        key: 'appLastUsed',
        dataIndex: 'appLastUsed',
        title: t('lastUsed'),
        sorter: true,
        filterDropdown: (props) => <DateFilter {...props} dateOnly={false} />,
        render: (_, p) => <FormattedDate value={p.appLastUsed} />,
    }];

    return (
        <Card title={t('customers')}>
            <AjaxTable<CustomerSummary>
                rowKey={(record) => record.id}
                columns={columns}
                fetchData={p => api.listCustomers(p)}
            />
        </Card>
    );
};

export default CustomerList;
