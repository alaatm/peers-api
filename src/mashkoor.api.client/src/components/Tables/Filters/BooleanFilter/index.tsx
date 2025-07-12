import { useState } from 'react';
import { Radio, Space } from 'antd';
import { FilterDropdownProps } from 'antd/es/table/interface';
import { RadioChangeEvent } from 'antd/es/radio';
import { useTranslation } from 'react-i18next';
import { FilterActions, serializeFilters, deserializeFilters, FunctionFilter } from '@/components/Tables';

/*
 * selectedKeys in this component will be following this pattern:
 * ['eq(true)']
 * -or-
 * ['eq(false)']
 */

const BooleanFilter = ({ setSelectedKeys, selectedKeys, confirm, clearFilters }: FilterDropdownProps) => {
    const { t } = useTranslation('components');
    const [keys, setKeys] = useState(deserializeFilters(selectedKeys));
    const [value, setValue] = useState(keys.length ? keys.filter(k => k.type === 'func').map(k => k.value.arg === 'true')[0] : undefined);

    const handleChange = (e: RadioChangeEvent) => {
        const newKey: FunctionFilter = { type: 'func', value: { func: 'eq', arg: e.target.value === 'true' } };
        setValue(e.target.value);
        setKeys([newKey]);
        setSelectedKeys(serializeFilters([newKey]));
    };

    const reset = () => {
        setValue(undefined);
        setKeys([]);
        clearFilters?.();
        confirm();
    };

    return (
        <Space className="filter-container" direction="vertical" size="middle">
            <Radio.Group onChange={handleChange} value={value}>
                <Radio value="true">{t('true')}</Radio>
                <Radio value="false">{t('false')}</Radio>
            </Radio.Group>
            <FilterActions filters={keys} onConfirm={() => confirm()} onReset={reset} />
        </Space>
    );
};

export default BooleanFilter;
