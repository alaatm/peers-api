import React from 'react';
import { Input, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import { LogicalDropdown, FuncDropdown, TypedFilterProps, FunctionFilter } from '@/components/Tables';

const Filter = ({ idx, filter, onChange, onRemove }: TypedFilterProps) => {
    const { t } = useTranslation('components');

    const handleArgChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const arg = e.target.value;
        const funcFilter = filter as FunctionFilter;
        onChange(idx, { ...funcFilter, value: { ...funcFilter.value, arg } });
    };

    return (
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            {filter.type === 'logical'
                ?
                <LogicalDropdown value={filter} onChange={(f) => onChange(idx, f)} onRemove={() => onRemove(idx)} />
                :
                <div className="flexrow">
                    <FuncDropdown funcs={['startswith', 'contains', 'endswith', 'eq', 'ne']} value={filter} onChange={(f) => onChange(idx, f)} />
                    <Input
                        style={{ width: '100%' }}
                        value={filter.value.arg as string | undefined}
                        placeholder={t('value')}
                        onChange={handleArgChange} />
                </div>
            }
        </Space>
    );
};

export default Filter;
