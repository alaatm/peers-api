import { useTranslation } from 'react-i18next';
import { Descriptions } from 'antd';
import { type Customer, UserStatus } from '@/api';
import { FormattedDate, Ltr } from '@/components';
import { te } from '@/utils';

type Props = {
    customer: Customer;
};

const DetailsBasicInfo = ({ customer }: Props) => {
    const { t } = useTranslation(['common', 'enums']);

    return (
        <Descriptions
            bordered
            column={1}>
            <Descriptions.Item label={t('name')}>{customer.name}</Descriptions.Item>
            <Descriptions.Item label={t('phoneNumber')}><Ltr value={customer.username} /></Descriptions.Item>
            <Descriptions.Item label={t('status')}>{te(t, UserStatus, customer.status)}</Descriptions.Item>
            <Descriptions.Item label={t('bookingsCount')}>{customer.bookingsCount}</Descriptions.Item>
            <Descriptions.Item label={t('lastUsed')}><FormattedDate value={customer.appLastUsed} includeTimeAgo={true} /></Descriptions.Item>
        </Descriptions>);
}

export default DetailsBasicInfo;
