import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Button, Dropdown, MenuProps } from 'antd';
import { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { FunctionName, FunctionFilter } from '@/components/Tables';

type MapValue = {
    label: string;
    icon: ReactNode;
    iconRtl?: ReactNode;
};

const funcsMap: Record<FunctionName, MapValue> = {
    startswith: { label: 'startsWith', icon: <FontAwesomeIcon icon="arrow-right-from-bracket" />, iconRtl: <FontAwesomeIcon icon="arrow-right-from-bracket" rotation={180} /> },
    contains: { label: 'contains', icon: <FontAwesomeIcon icon="font" /> },
    endswith: { label: 'endsWith', icon: <FontAwesomeIcon icon="arrow-right-to-bracket" />, iconRtl: <FontAwesomeIcon icon="arrow-right-to-bracket" rotation={180} /> },
    lt: { label: 'lessThan', icon: <FontAwesomeIcon icon="less-than" />, iconRtl: <FontAwesomeIcon icon="less-than" rotation={180} /> },
    le: { label: 'lessThanOrEq', icon: <FontAwesomeIcon icon="less-than-equal" />, iconRtl: <FontAwesomeIcon icon="less-than-equal" flip="horizontal" /> },
    eq: { label: 'eq', icon: <FontAwesomeIcon icon="equals" /> },
    ge: { label: 'greaterThanOrEq', icon: <FontAwesomeIcon icon="greater-than-equal" />, iconRtl: <FontAwesomeIcon icon="greater-than-equal" flip="horizontal" /> },
    gt: { label: 'greaterThan', icon: <FontAwesomeIcon icon="greater-than" />, iconRtl: <FontAwesomeIcon icon="greater-than" rotation={180} /> },
    ne: { label: 'notEq', icon: <FontAwesomeIcon icon="not-equal" /> },
};

type Props = {
    funcs: FunctionName[];
    value: FunctionFilter;
    onChange: (value: FunctionFilter) => void;
};

const FuncDropdown = ({ funcs, value, onChange }: Props) => {
    const { t, i18n } = useTranslation('components');
    const isRtl = i18n.dir() === 'rtl';

    const items: MenuProps['items'] = funcs.map(f => {
        const map = funcsMap[f];

        return {
            key: f,
            icon: isRtl ? map.iconRtl ?? map.icon : map.icon,
            label: t(map.label),
            onClick: () => onChange({
                type: 'func',
                value: {
                    ...value.value,
                    func: f,
                },
            } as FunctionFilter),
        };
    });

    const currValue = funcsMap[value.value.func];

    return (
        <Dropdown menu={{ items }} trigger={['click']}>
            <Button>
                {isRtl ? currValue.iconRtl ?? currValue.icon : currValue.icon}
            </Button>
        </Dropdown>
    );
};

export default FuncDropdown
