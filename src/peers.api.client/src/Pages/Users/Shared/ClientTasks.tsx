import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Descriptions, Button } from 'antd';
import { api, ClientTaskAcknowledgeStatus, ClientTaskRequestTaskType, isError, isOk } from '@/api';
import { te } from '@/utils';

type Task = { name: string, task: ClientTaskRequestTaskType, canExecute: () => Promise<boolean>, executing: boolean, result: string | null };


type Props = {
    deviceId: string;
}

const ClientTasks = ({ deviceId }: Props) => {
    const { t } = useTranslation(['common', 'enums']);
    const intervalRef = useRef<NodeJS.Timeout | null>(null);
    const [, setRetryCount] = useState(0);
    const [tasks, setTasks] = useState<Task[]>([
        {
            name: te(t, ClientTaskRequestTaskType, ClientTaskRequestTaskType.Ping),
            task: ClientTaskRequestTaskType.Ping,
            canExecute: () => Promise.resolve(true),
            executing: false,
            result: null,
        },
        {
            name: te(t, ClientTaskRequestTaskType, ClientTaskRequestTaskType.SignOff),
            task: ClientTaskRequestTaskType.SignOff,
            canExecute: () => Promise.resolve(true),
            executing: false,
            result: null,
        },
    ]);

    // Clear interval on unmount
    useEffect(() => {
        return () => {
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
            }
        };
    }, []);

    const clientTaskRequest = async (task: Task) => {
        task.executing = true;
        task.result = null;
        setTasks([...tasks]);

        const canExecute = await task.canExecute();
        if (!canExecute) {
            task.executing = false;
            task.result = t('cannotSignOut');
            setTasks([...tasks]);
            return;
        }

        const reqResponse = await api.sendClientTaskRequest(deviceId, task.task);
        if (isError(reqResponse)) {
            task.executing = false;
            task.result = t('requestFailed');
            setTasks([...tasks]);
            return;
        }

        const requestId = reqResponse.requestId;

        // Clear any existing interval to avoid overlapping retries
        if (intervalRef.current) {
            clearInterval(intervalRef.current);
        }
        setRetryCount(0);
        intervalRef.current = setInterval(async () => {
            setRetryCount(prevCount => {
                // If we've hit our max retries, clear the interval
                if (prevCount >= 9) {  // since we start at 0, count 0..9 is 10 retries
                    if (intervalRef.current) clearInterval(intervalRef.current);
                    task.executing = false;
                    task.result = t('requestTimedOut');
                    setTasks([...tasks]);
                    return prevCount + 1;
                }

                (async () => {
                    const ackResponse = await api.checkClientTaskRequest(deviceId, requestId);
                    if (isOk(ackResponse)) {
                        const ackStatus = ackResponse.status;
                        task.result = te(t, ClientTaskAcknowledgeStatus, ackStatus);
                        setTasks([...tasks]);

                        if (ackStatus === ClientTaskAcknowledgeStatus.Ok || ackStatus === ClientTaskAcknowledgeStatus.NoResponse) {
                            task.executing = false;
                            setTasks([...tasks]);
                            if (intervalRef.current) clearInterval(intervalRef.current);
                        }
                    } else {
                        if (intervalRef.current) clearInterval(intervalRef.current);
                        task.executing = false;
                        task.result = t('requestFailedWithDetails', { error: ackResponse.detail });
                        setTasks([...tasks]);
                    }
                })();

                return prevCount + 1;
            });
        }, 5000);
    };

    return <Descriptions
        bordered
        styles={{ label: { width: '10em' } }}
        size="small"
        column={1}>
        {tasks.map((t, i) => (
            <Descriptions.Item
                key={i}
                label={
                    <Button
                        style={{ width: '7em' }}
                        type="primary"
                        loading={t.executing}
                        disabled={tasks.some(p => p.task !== t.task && p.executing)}
                        onClick={() => clientTaskRequest(t)}>
                        {t.name}
                    </Button>}>
                {t.result}
            </Descriptions.Item>
        ))}</Descriptions>;
};

export default ClientTasks;
