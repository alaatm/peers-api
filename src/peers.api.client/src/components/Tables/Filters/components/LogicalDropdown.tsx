import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Button, Select, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import type { LogicalFilter, LogicalOperation } from '@/components/Tables';

type Props = {
    value: LogicalFilter;
    onChange: (value: LogicalFilter) => void;
    onRemove: () => void;
};

const LogicalDropdown = ({ value, onChange, onRemove }: Props) => {
    const { t } = useTranslation('components');

    const handleChange = (value: LogicalOperation) => {
        onChange({
            type: 'logical',
            value,
        });
    };

    return (
        <Space size="small">
            <Select
                className="logical-dropdown"
                defaultValue="and"
                value={value.value}
                onChange={handleChange}>
                <Select.Option value="and">{t('and')}</Select.Option>
                <Select.Option value="or">{t('or')}</Select.Option>
            </Select>
            <Button danger title={t('remove')} onClick={onRemove}><FontAwesomeIcon icon="minus-circle" /></Button>
        </Space>
    );
};

export default LogicalDropdown
