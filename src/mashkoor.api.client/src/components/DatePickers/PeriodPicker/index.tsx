import dayjs from 'dayjs';
import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Col, DatePicker, InputNumber, Row, Select, Space } from 'antd';
import { PeriodKind } from '@/api';
import { enumToOptionsTrans } from '@/utils';
import { getPickerForKind, initialPeriodData, isInitialPeriodData, isSamePeriodData, PeriodData } from '../common';
import './index.css';

type Props = {
    allowMulti?: boolean;
    disabled?: boolean;
    onApply: (data: PeriodData) => void;
}

const PeriodPicker = ({ onApply, disabled, allowMulti = true }: Props) => {
    const { t } = useTranslation(['components', 'enums']);

    const [formData, setFormData] = useState<PeriodData>(initialPeriodData);
    const [lastApplyData, setLastApplyData] = useState<PeriodData>(initialPeriodData);

    const changedSinceLastApply = useMemo(() => !isSamePeriodData(formData, lastApplyData), [formData, lastApplyData]);
    const isReset = useMemo(() => isInitialPeriodData(formData), [formData]);
    const getPicker = useCallback(() => getPickerForKind(formData.kind), [formData.kind]);

    const updatePeriodKind = (kind: PeriodKind) => {
        setFormData((prev) => ({
            ...prev,
            kind: kind,
            date: prev.date.startOf(getPickerForKind(kind)),
            customNumDays: kind === PeriodKind.Custom ? 1 : null,
        }));
    };

    const updateDate = (value?: dayjs.Dayjs) => {
        setFormData((prev) => ({
            ...prev,
            date: value || initialPeriodData.date,
        }));
    }

    const updateCustomNumDays = (value: number | null) => {
        setFormData((prev) => ({
            ...prev,
            customNumDays: value || initialPeriodData.customNumDays,
        }));
    };

    const updateNumPeriods = (value: number | null) => {
        setFormData((prev) => ({
            ...prev,
            numPeriods: value || initialPeriodData.numPeriods,
        }));
    }

    const apply = useCallback(() => {
        onApply(formData);
        setLastApplyData(formData);
    }, [formData, onApply]);

    return (
        <div className="period-picker">
            <Row className="row">
                <Col span={8}>{t('datePickers.periodKind')}</Col>
                <Col span={8}>
                    <Select
                        value={formData.kind}
                        onChange={updatePeriodKind}
                        options={enumToOptionsTrans(t, PeriodKind)}
                        style={{ width: '100%' }} />
                </Col>
            </Row>
            <Row className="row">
                <Col span={8}>{t('datePickers.date')}</Col>
                <Col span={8}>
                    <DatePicker className="input" disabled={disabled} value={formData.date} onChange={updateDate} picker={getPicker()} />
                </Col>
            </Row>
            {formData.kind === PeriodKind.Custom && (
                <Row className="row">
                    <Col span={8}>{t('datePickers.customNumDays')}</Col>
                    <Col span={8}>
                        <InputNumber className="input" disabled={disabled} min={1} max={365} value={formData.customNumDays} onChange={updateCustomNumDays} />
                    </Col>
                </Row>
            )}
            {allowMulti && <Row className="row">
                <Col span={8}>{t('datePickers.numPeriods')}</Col>
                <Col span={8}>
                    <InputNumber className="input" disabled={disabled} min={1} max={7} value={formData.numPeriods} onChange={updateNumPeriods} />
                </Col>
            </Row>}
            <Row>
                <Col offset={8} span={8}>
                    <Space>
                        <Button disabled={!changedSinceLastApply || disabled} type="primary" onClick={apply}>{t('datePickers.apply')}</Button>
                        <Button disabled={isReset || disabled} type="default" onClick={() => setFormData(initialPeriodData)}>{t('datePickers.reset')}</Button>
                    </Space>
                </Col>
            </Row>
        </div>
    )
}

export default PeriodPicker;
