import React from 'react';
import { Form, Input, FormItemProps } from 'antd';
import { Lang } from '@/api';
import { useTranslation } from 'react-i18next';
import { supportedLangs } from '@/Language';

type Props = {
    namePrefix: string;
    labelPrefix: string;
    rules?: FormItemProps['rules'];
    inputProps?: React.ComponentProps<typeof Input> | ((lang: Lang) => React.ComponentProps<typeof Input>);
}

const MultiLangFormItem = ({
    namePrefix,
    labelPrefix,
    rules,
    inputProps,
}: Props) => {
    const { t } = useTranslation('components');

    return (
        <>
            {supportedLangs.map((lang) => {
                const fieldName = `${namePrefix}_${lang.code}`;
                const label = `${labelPrefix} (${lang.name})`;

                const baseInputProps: React.ComponentProps<typeof Input> = {
                    placeholder: t('multiLangFormItem.placeholder', { field: labelPrefix.toLowerCase(), lang: lang.name }),
                };

                const additionalProps =
                    typeof inputProps === 'function' ? inputProps(lang) : inputProps || {};

                return (
                    <Form.Item
                        key={fieldName}
                        label={label}
                        name={fieldName}
                        rules={rules}
                    >
                        <Input {...baseInputProps} {...additionalProps} />
                    </Form.Item>
                );
            })}
        </>
    );
};

export default MultiLangFormItem;
