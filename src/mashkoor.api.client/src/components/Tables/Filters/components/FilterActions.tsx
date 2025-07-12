import { Button } from 'antd';
import { useTranslation } from 'react-i18next';
import { isValidFilter, Filter as GenericFilter } from '@/components/Tables';

type Props = {
    filters: GenericFilter[];
    onConfirm: () => void;
    onReset: () => void;
}

const FilterActions = ({ filters, onConfirm, onReset }: Props) => {
    const { t } = useTranslation('components');

    return (
        <div className="actions">
            <Button type="primary" disabled={filters.length == 0 || filters.some(p => !isValidFilter(p))} onClick={onConfirm}>{t('filter')}</Button>
            <Button danger onClick={onReset}>{t('clear')}</Button>
        </div>
    );
};

export default FilterActions;
