import dayjs from 'dayjs';
import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Col, DatePicker, Row, Space } from 'antd';
import { MinusCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { initialMultiDateData, isSameMultiDateData } from '../common';
import './index.css';

type Props = {
    disabled?: boolean;
    onApply: (dates: dayjs.Dayjs[]) => void;
}

const MultiDatePicker = ({ onApply, disabled }: Props) => {
    const { t } = useTranslation('components');
    const [dates, setDates] = useState<(dayjs.Dayjs | undefined)[]>(initialMultiDateData);
    const [lastApplyData, setLastApplyData] = useState(initialMultiDateData);
    const changedSinceLastApply = useMemo(() => !isSameMultiDateData(dates, lastApplyData), [dates, lastApplyData]);
    const formValid = () => dates.every(d => d !== undefined);

    const add = () => setDates([...dates, undefined]);

    const update = useCallback((index: number, value: dayjs.Dayjs) => {
        setDates(prev => {
            const newDates = [...prev];
            newDates[index] = value;
            return newDates;
        });
    }, []);

    const remove = useCallback((index: number) => () => {
        setDates(prevDates => {
            const newDates = [...prevDates];
            newDates.splice(index, 1);
            return newDates;
        });
    }, []);


    const apply = useCallback(() => {
        onApply(dates as dayjs.Dayjs[]);
        setLastApplyData(dates as dayjs.Dayjs[]);
    }, [dates, onApply]);

    return (
        <div className="multi-date-picker">
            {dates.map((_, i) => (
                <Row className="row" key={i}>
                    <Col span={8}>{t('datePickers.asOfDateIndex', { index: i + 1 })}</Col>
                    <Col span={8}>
                        <DatePicker className="input" disabled={disabled} value={dates[i]} onChange={(date,) => update(i, date)} />
                    </Col>
                    {i > 0 ? (
                        <Col span={1}>
                            <MinusCircleOutlined
                                className="dynamic-delete-button"
                                disabled={disabled}
                                onClick={remove(i)}
                            />
                        </Col>
                    ) : null}
                </Row>
            ))}
            <Row className="row">
                <Col offset={8} span={8}>
                    <Button
                        className="input"
                        disabled={disabled}
                        type="dashed"
                        onClick={add}>
                        <PlusOutlined />
                        {t('datePickers.addDate')}
                    </Button>
                </Col>
            </Row>
            <Row>
                <Col offset={8} span={8}>
                    <Space>
                        <Button disabled={!changedSinceLastApply || !formValid() || disabled} type="primary" onClick={apply}>{t('datePickers.apply')}</Button>
                    </Space>
                </Col>
            </Row>
        </div>
    );
};

export default MultiDatePicker;
