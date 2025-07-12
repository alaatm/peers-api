import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Card, Form, Input, Modal } from 'antd';
import { api, isError, ProblemDetails } from '@/api';
import { Problem } from '@/components';
import { useAuth } from '@/Auth';

type FormValuesType = {
    currentPassword: string;
    newPassword: string;
    newPasswordConfirm: string;
};

const ChangePasswordPage = () => {
    const { t } = useTranslation('forms', { keyPrefix: 'fields' });
    const { signout } = useAuth();
    const [form] = Form.useForm<FormValuesType>();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<ProblemDetails>();
    const [modal, contextHolder] = Modal.useModal();

    const onFinish = async (values: FormValuesType) => {
        setError(undefined);
        setLoading(true);
        const response = await api.changePassword(values.currentPassword, values.newPassword, values.newPasswordConfirm);
        if (isError(response)) {
            setError(response);
        } else {
            form.resetFields();
            await modal.success({
                title: t('success', { keyPrefix: 'changePassword' }),
                content: t('successDescr', { keyPrefix: 'changePassword' }),
            });
            signout();
        }
        setLoading(false);
    };

    return (
        <Card title={t('title', { keyPrefix: 'changePassword' })}>
            <Problem problem={error} />

            <Form
                form={form}
                disabled={loading}
                name="change-password"
                style={{ maxWidth: 600 }}
                onFinish={onFinish}
                layout="vertical"
            >
                <Form.Item
                    name="currentPassword"
                    label={t('currentPassword.label')}
                    rules={[{ required: true }]}>
                    <Input.Password
                        autoComplete="new-password"
                        placeholder={t('currentPassword.placeholder')}
                    />
                </Form.Item>

                <Form.Item
                    name="newPassword"
                    label={t('newPassword.label')}
                    rules={[{ required: true }]}>
                    <Input.Password
                        autoComplete="new-password"
                        placeholder={t('newPassword.placeholder')}
                    />
                </Form.Item>

                <Form.Item
                    name="newPasswordConfirm"
                    label={t('newPasswordConfirm.label')}
                    dependencies={['newPassword']}
                    rules={[
                        {
                            required: true,
                        },
                        ({ getFieldValue }) => ({
                            validator(_, value) {
                                if (!value || getFieldValue('newPassword') === value) {
                                    return Promise.resolve();
                                }
                                return Promise.reject(new Error(t('newPasswordConfirm.invalid')));
                            },
                        }),
                    ]}
                >
                    <Input.Password
                        autoComplete="new-password"
                        placeholder={t('newPasswordConfirm.placeholder')}
                    />
                </Form.Item>

                <Form.Item>
                    <Button type="primary" htmlType="submit" loading={loading}>
                        {t('submit')}
                    </Button>
                </Form.Item>

            </Form>

            {contextHolder}
        </Card>
    );
};

export default ChangePasswordPage;
