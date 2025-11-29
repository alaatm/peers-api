import { Alert, Divider, Space, Typography } from 'antd';
import type { ProblemDetails } from '@/api';
import type { JSX } from 'react';
import { useTranslation } from 'react-i18next';

type Props = {
    problem?: ProblemDetails | null;
    includeDivider?: boolean;
}

const Problem = ({ problem, includeDivider = true }: Props) => {
    const { t, i18n } = useTranslation();

    if (!problem) {
        return null;
    }

    let errors: JSX.Element[] = [];

    if (problem.errors instanceof Array) {
        errors = problem.errors!.map((e, i) => <Typography.Text key={i}>{e}</Typography.Text>);
    } else if (problem.errors) {
        errors = Object.entries(problem.errors).map(([key, value], i) => {
            return (
                <Space key={i} align="start" size="large">
                    <Typography.Text strong>{key}:</Typography.Text>
                    {value instanceof Array ? (
                        <Space direction="vertical">
                            {value.map((v, j) => (
                                <Typography.Text key={j}>{v}</Typography.Text>
                            ))}
                        </Space>
                    ) : (
                        <Typography.Text>{value}</Typography.Text>
                    )}
                </Space>
            );
        });
    }

    const message = problem.detail ?? problem.title ?? 'An error occurred';
    const translatedMessage = i18n.exists(message) ? t(message) : message;

    return problem ? (
        <>
            <Alert
                type="error"
                showIcon={true}
                message={translatedMessage}
                description={<Space direction="vertical">{errors}</Space>}
            />
            {includeDivider && <Divider />}
        </>
    ) : null;
};

export default Problem;
