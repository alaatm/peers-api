import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal, Input } from 'antd';
import { api, isOk, ProblemDetails, UserStatus } from '@/api';
import { Problem } from '@/components';

type Props = {
    open: boolean;
    userId: number;
    targetStatus: UserStatus;
    onOk: () => void;
    onCancel: () => void;
};

const StatusChangeDialog = ({ open, userId, targetStatus, onOk, onCancel }: Props) => {
    const { t } = useTranslation();
    const [changeReason, setChangeReason] = useState<string>('');
    const [saving, setSaving] = useState<boolean>(false);
    const [error, setError] = useState<ProblemDetails>();

    const changeStatus = async () => {
        setError(undefined);
        setSaving(true);
        const response = await api.setUserStatus(userId, targetStatus, changeReason);

        if (isOk(response)) {
            setChangeReason('');
            onOk();
        }
        else {
            setError(response);
        }

        setSaving(false);
    };

    const title = targetStatus === UserStatus.Active
        ? t('reactivateUser')
        : targetStatus === UserStatus.Suspended
            ? t('suspendUser')
            : targetStatus === UserStatus.Banned
                ? t('banUser')
                : '???';

    return (
        <Modal
            title={title}
            open={open}
            onOk={changeStatus}
            onCancel={() => { setError(undefined); setChangeReason(''); onCancel(); }}
            confirmLoading={saving}
            okText={t('change')}
            cancelText={t('cancel')}

        >
            <Problem problem={error} />

            <p>{t('userStatusChangePrompt', { newStatus: title.toLocaleLowerCase() })}</p>
            <Input.TextArea value={changeReason} rows={5} placeholder={t('userStatusChangeReasonPlaceholder')} maxLength={1024} onChange={(e) => setChangeReason(e.target.value)} />
        </Modal>
    );
};

export default StatusChangeDialog;
