import { useRef, useState } from 'react';
import { useTranslation, Trans } from 'react-i18next';
import { Alert, Button, Card, Form, Input, notification, Radio, Space } from 'antd';
import data from '@emoji-mart/data';
import Picker from '@emoji-mart/react';
import i18n from '@emoji-mart/data/i18n/ar.json';
import { api, DeviceGroup, isError } from '@/api';
import { MultiLangFormItem } from '@/components';
import { supportedLangs, useLang } from '@/Language';

type FormValuesType = {
    target: DeviceGroup;
    deviceToken?: string;
    [key: string]: string | DeviceGroup | undefined;
}


const DevicePush = () => {
    const { t } = useTranslation(['common', 'enums']);
    const { lang: { code: langCode } } = useLang();
    const [form] = Form.useForm<FormValuesType>();
    const [saving, setSaving] = useState(false);
    const [antdNotification, contextHolder] = notification.useNotification();
    const [focusedField, setFocusedField] = useState<string | null>(null);
    const focusedInputRef = useRef<HTMLInputElement | null>(null);

    const handleEmojiSelect = (emoji: { native: string; }) => {
        if (!focusedField || !focusedInputRef.current) return;

        const input = focusedInputRef.current;
        const start = input.selectionStart ?? 0;
        const end = input.selectionEnd ?? 0;
        const currentValue = input.value;
        const emojiStr = emoji.native;

        // Build the new value with the emoji inserted at the cursor
        const newValue = currentValue.slice(0, start) + emojiStr + currentValue.slice(end);

        // Update the form field value using Antd's API
        form.setFieldValue(focusedField, newValue);

        // Reset the cursor position after updating the value
        setTimeout(() => {
            input.focus();
            input.setSelectionRange(start + emojiStr.length, start + emojiStr.length);
        }, 0);
    };

    const onFinish = async (values: FormValuesType) => {
        setSaving(true);

        const title: Record<string, string> = {};
        const body: Record<string, string> = {};

        for (const lang of supportedLangs) {
            title[lang.code] = (values[`title_${lang.code}`] as string).trim();
            body[lang.code] = (values[`body_${lang.code}`] as string).trim();
        }

        const response = await api.sendPushNotification({
            deviceToken: values.target === DeviceGroup.None ? values.deviceToken : null,
            deviceGroup: values.target,
            title: title,
            body: body,
        });

        setSaving(false);
        if (isError(response)) {
            antdNotification.error({
                duration: 0,
                message: 'Message Send Error',
                description: response.detail ?? 'An unknown error occurred.',
                placement: 'top',
            });
        } else {
            antdNotification.info({
                duration: 0,
                message: 'Message Sent',
                description: 'Your message has been dispatched with no guraantee of delivery.',
                placement: 'top',
            });
        }
    };

    const reset = () => {
        form.resetFields();
    };

    return (
        <>
            {contextHolder}
            <Card title={t('devicePush.title')}>
                <Alert
                    style={{ marginBottom: '1em' }}
                    message={<b>{t('devicePush.alertTitle')}</b>}
                    description={
                        <Trans
                            i18nKey="devicePush.alertDescr"
                            components={{ div: <div />, ul: <ul />, li: <li />, b: <b /> }}
                        />
                    }
                    type="warning"
                    showIcon
                />

                <Form
                    form={form}
                    labelCol={{ span: 2 }}
                    wrapperCol={{ span: 12 }}
                    layout="horizontal"
                    name="device-push"
                    disabled={saving}
                    initialValues={{ target: DeviceGroup.None }}
                    onFinish={onFinish}
                >
                    <Form.Item
                        label={t('devicePush.target')}
                        name="target"
                        rules={[{ required: true, message: t('devicePush.targetRqMsg') }]}>
                        <Radio.Group>
                            <Radio value={DeviceGroup.Customers}>{t('customers')}</Radio>
                            <Radio value={DeviceGroup.Dispatchers}>{t('dispatchers')}</Radio>
                            <Radio value={DeviceGroup.Drivers}>{t('drivers')}</Radio>
                            <Radio value={DeviceGroup.ManagedDrivers}>{t('managedDrivers')}</Radio>
                            <Radio value={DeviceGroup.All}>{t('devicePush.allExclManagedDrivers')}</Radio>
                            <Radio value={DeviceGroup.None}>{t('devicePush.specificDevice')}</Radio>
                        </Radio.Group>
                    </Form.Item>

                    <Form.Item
                        dependencies={['target']} noStyle>
                        {({ getFieldValue }) =>
                            getFieldValue('target') === DeviceGroup.None ? (
                                <Form.Item
                                    label={t('devicePush.deviceToken')}
                                    name="deviceToken"
                                    rules={[{ required: true, message: t('devicePush.deviceTokenRqMsg') }]}
                                >
                                    <Input
                                        style={{ width: 350 }}
                                        placeholder={t('devicePush.deviceTokenPlaceholder')}
                                    />
                                </Form.Item>
                            ) : null
                        }
                    </Form.Item>

                    <MultiLangFormItem
                        namePrefix="title"
                        labelPrefix={t('devicePush.msgTitle')}
                        rules={[{ required: true, message: t('devicePush.msgTitleRqMsg') }]}
                        inputProps={(lang) => ({
                            onFocus: (e) => {
                                setFocusedField(`title_${lang.code}`);
                                focusedInputRef.current = e.target;
                            },
                        })}
                    />

                    <MultiLangFormItem
                        namePrefix="body"
                        labelPrefix={t('devicePush.msgBody')}
                        rules={[{ required: true, message: t('devicePush.msgBodyRqMsg') }]}
                        inputProps={(lang) => ({
                            onFocus: (e) => {
                                setFocusedField(`body_${lang.code}`);
                                focusedInputRef.current = e.target;
                            },
                        })}
                    />

                    <Form.Item wrapperCol={{ span: 12, offset: 2 }}>
                        <Picker
                            locale={langCode}
                            i18n={langCode === 'ar' ? i18n : undefined}
                            data={data}
                            perLine={18}
                            theme="light"
                            previewPosition="none"
                            categories={['frequent', 'people', 'nature', 'foods', 'activity', 'places', 'objects', 'symbols']}
                            onEmojiSelect={handleEmojiSelect} />
                    </Form.Item>

                    <Form.Item wrapperCol={{ span: 16, offset: 2 }}>
                        <Space>
                            <Button
                                loading={saving}
                                type="primary"
                                htmlType="submit">{t('send')}</Button>
                            <Button htmlType="button" onClick={reset}>{t('reset')}</Button>
                        </Space>
                    </Form.Item>

                </Form>
            </Card>
        </>
    );
};

export default DevicePush;
