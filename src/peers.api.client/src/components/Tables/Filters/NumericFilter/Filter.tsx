import { InputNumber, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import { LogicalDropdown, FuncDropdown, type TypedFilterProps, type FunctionFilter } from '@/components/Tables';

const Filter = ({ idx, filter, onChange, onRemove }: TypedFilterProps) => {
    const { t } = useTranslation('components');

    const handleArgChange = (value: number | null) => {
        const funcFilter = filter as FunctionFilter;
        onChange(idx, { ...funcFilter, value: { ...funcFilter.value, arg: value ?? undefined } });
    };

    return (
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            {filter.type === 'logical'
                ?
                <LogicalDropdown value={filter} onChange={(f) => onChange(idx, f)} onRemove={() => onRemove(idx)} />
                :
                <div className="flexrow">
                    <FuncDropdown funcs={['lt', 'le', 'eq', 'gt', 'ge', 'ne']} value={filter} onChange={(f) => onChange(idx, f)} />
                    <InputNumber
                        style={{ width: '100%' }}
                        value={filter.value.arg as number | undefined}
                        placeholder={t('value')}
                        onChange={handleArgChange} />
                </div>
            }
        </Space>
    );
};

export default Filter;
