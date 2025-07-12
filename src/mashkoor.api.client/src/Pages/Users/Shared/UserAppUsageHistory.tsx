import { useTranslation } from 'react-i18next';
import { v4 as uuidv4 } from 'uuid';
import { api, AppUsageHistorySummary } from '@/api';
import { FormattedDate } from '@/components';
import { AjaxTable } from '@/components/Tables';


type Props = {
    userId: number;
};

const UserAppUsageHistory = ({ userId }: Props) => {
    const { t } = useTranslation([]);

    return (
        <AjaxTable<AppUsageHistorySummary>
            rowKey={_ => uuidv4()}
            columns={[{
                key: 'openedAt',
                dataIndex: 'openedAt',
                title: t('openedAt'),
                sorter: true,
                render: (_, p) => <FormattedDate value={p.openedAt} />,
            }]}
            fetchData={p => api.listUserAppUsageHistory(userId, p)}
        />
    );
}

export default UserAppUsageHistory;
