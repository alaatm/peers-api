import { useState } from 'react';
import { Checkbox, Select, Space } from 'antd';
import { FilterDropdownProps } from 'antd/es/table/interface';
import { useTranslation } from 'react-i18next';
import { FilterActions, deserializeFilters, serializeFilters } from '@/components/Tables';

/*
 * selectedKeys in this component will be following this pattern:
 * ['eq(value1)', 'or', 'eq(value2)', 'or', ...]
 */

interface Props extends FilterDropdownProps {
    type?: 'check' | 'select';
    options: { label: string, value: string | number }[];
    onFilterApply?: (selectedValues: string[]) => void;
}

const OptionsFilter = ({ setSelectedKeys, selectedKeys, confirm, clearFilters, type, options, onFilterApply }: Props) => {
    const { t } = useTranslation('components');
    const [keys, setKeys] = useState(deserializeFilters(selectedKeys));
    const [values, setValues] = useState(keys.filter(k => k.type === 'func').map(k => k.value.arg as string));

    const handleChange = (checkedValues: string[]) => {
        const count = (checkedValues.length * 2) - 1;
        if (count <= 0) {
            setValues([]);
            setKeys([]);
            setSelectedKeys([]);
        } else {
            const newKeys = Array(count).fill({ type: 'logical', value: 'or' });

            for (let i = 0; i < count; i += 2) {
                newKeys[i] = { type: 'func', value: { func: 'eq', arg: checkedValues[i / 2] } };
            }

            setValues(checkedValues);
            setKeys(newKeys);
            setSelectedKeys(serializeFilters(newKeys));
        }
    };

    const handleConfirm = () => {
        onFilterApply?.(values);
        confirm();
    };

    const reset = () => {
        setValues([]);
        setKeys([]);
        clearFilters?.();
        confirm();
        onFilterApply?.([]);
    };

    return (
        <Space className="filter-container" direction="vertical" size="middle">
            {(type || 'select') === 'check' && <Checkbox.Group
                className="filter"
                value={values}
                onChange={handleChange}
                options={options.map(p => ({ label: p.label, value: String(p.value) }))} />}
            {(type || 'select') === 'select' && <Select
                mode="multiple"
                style={{ width: '100%' }}
                placeholder={t('selectValue')}
                options={options}
                value={values}
                filterOption={(input, option) => (option?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                onChange={handleChange} />}

            <FilterActions filters={keys} onConfirm={handleConfirm} onReset={reset} />
        </Space>
    );
};

export default OptionsFilter;
