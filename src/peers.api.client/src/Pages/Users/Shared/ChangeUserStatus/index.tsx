import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Space } from 'antd';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { UserStatus } from '@/api';
import StatusChangeDialog from './StatusChangeDialog';

type Props = {
    userId: number;
    status: UserStatus;
    onStatusChange: () => void;
};

const ChangeUserStatus = ({ userId, status, onStatusChange }: Props) => {
    const { t } = useTranslation();
    const [dialogOpen, setDialogOpen] = useState(false);
    const [targetStatus, setTargetStatus] = useState<UserStatus>();

    useEffect(() => {
        if (targetStatus) {
            setDialogOpen(true);
        }
    }, [targetStatus]);

    const renderActions = () => {
        if (status === UserStatus.Active) {
            return (
                <Space>
                    <Button danger type="primary" icon={<FontAwesomeIcon icon="ban" />} onClick={() => setTargetStatus(UserStatus.Suspended)}>
                        {t('suspend')}
                    </Button>
                    <Button danger type="primary" icon={<FontAwesomeIcon icon="user-slash" />} onClick={() => setTargetStatus(UserStatus.Banned)}>
                        {t('ban')}
                    </Button>
                </Space>
            );
        } else if (status === UserStatus.Suspended || status === UserStatus.Banned) {
            return (
                <Button type="primary" onClick={() => setTargetStatus(UserStatus.Active)}>
                    {t('reactivate')}
                </Button>
            );
        }

        return null;
    };

    return (
        <>
            {renderActions()}
            <StatusChangeDialog
                open={dialogOpen}
                userId={userId}
                targetStatus={targetStatus!}
                onOk={() => { setDialogOpen(false); onStatusChange(); }}
                onCancel={() => setDialogOpen(false)} />
        </>
    )
};

export default ChangeUserStatus;
