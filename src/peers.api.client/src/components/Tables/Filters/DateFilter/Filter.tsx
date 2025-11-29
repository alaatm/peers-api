import dayjs from 'dayjs';
import { DatePicker, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import { FuncDropdown, LogicalDropdown, type TypedFilterProps, type FunctionFilter } from '@/components/Tables';

const Filter = ({ idx, filter, onChange, onRemove }: TypedFilterProps) => {
    const { t } = useTranslation('components');

    const handleArgChange = (value?: dayjs.Dayjs) => {
        const funcFilter = filter as Extract<FunctionFilter, { value: { arg: dayjs.Dayjs | undefined } }>;

        onChange(idx, {
            ...funcFilter,
            value: {
                ...funcFilter.value,
                arg: value,
            },
        });
    };

    return (
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            {filter.type === 'logical'
                ?
                <LogicalDropdown value={filter} onChange={(f) => onChange(idx, f)} onRemove={() => onRemove(idx)} />
                :
                <div className="flexrow">
                    <FuncDropdown funcs={['lt', 'le', 'eq', 'gt', 'ge', 'ne']} value={filter} onChange={(f) => onChange(idx, f)} />
                    <DatePicker
                        style={{ width: '100%' }}
                        value={filter.value.arg as dayjs.Dayjs | undefined}
                        placeholder={t('value')}
                        onChange={handleArgChange} />
                </div>
            }
        </Space>
    );
};

export default Filter;
