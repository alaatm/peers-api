import { useState, type ComponentType } from 'react';
import { Button, Space } from 'antd';
import type { FilterDropdownProps } from 'antd/es/table/interface';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useTranslation } from 'react-i18next';
import { FilterActions, type TypedFilterProps, getInitialFilterState, serializeFilters, type Filter as GenericFilter } from '@/components/Tables';

export interface BaseFilterProps extends FilterDropdownProps {
    TypedFilter: ComponentType<TypedFilterProps>;
    dateOnly?: boolean;
}

const BaseFilter = ({ setSelectedKeys, selectedKeys, confirm, clearFilters, TypedFilter, dateOnly }: BaseFilterProps) => {
    const { t } = useTranslation('components');
    const [keys, setKeys] = useState(getInitialFilterState(selectedKeys, dateOnly));

    const handleFilterChange = (index: number, filter: GenericFilter) => {
        const newKeys = keys.map((p, i) => (i === index ? filter : p));
        setKeys(newKeys);
        setSelectedKeys(serializeFilters(newKeys));
    };

    const handleFilterRemove = (idx: number) => {
        const newKeys = [...keys.slice(0, idx)];
        setKeys(newKeys);
        setSelectedKeys(serializeFilters(newKeys));
    };

    const addFilter = () => {
        setKeys([
            ...keys,
            { type: 'logical', value: 'and' },
            { type: 'func', value: { func: 'eq', arg: undefined, dateOnly } }
        ]);
    };

    const reset = () => {
        setKeys(getInitialFilterState([], dateOnly));
        clearFilters?.();
        confirm();
    };

    return (
        <Space className="filter-container" direction="vertical" size="middle">
            {keys.map((k, i) => <TypedFilter key={i} onChange={handleFilterChange} onRemove={handleFilterRemove} idx={i} filter={k} />)}
            {keys.length < 2 && <Button className="add" type="dashed" onClick={addFilter}>
                <FontAwesomeIcon icon="plus" />{t('addFilter')}
            </Button>}
            <FilterActions filters={keys} onConfirm={() => confirm()} onReset={reset} />
        </Space>
    );
};

export default BaseFilter;
