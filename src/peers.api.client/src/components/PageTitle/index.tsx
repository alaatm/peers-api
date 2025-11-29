import type { ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { PageHeader, type PageHeaderProps } from '@ant-design/pro-components';
import BackButton from '../BackButton';

type Props = {
    title: ReactNode;
    subTitle?: ReactNode;
    tags?: PageHeaderProps['tags'];
}

const PageTitle = ({ title, subTitle, tags }: Props) => {
    const navigate = useNavigate();

    return (<PageHeader
        tags={tags}
        ghost={false}
        title={title}
        subTitle={subTitle}
        backIcon={<BackButton />}
        onBack={() => navigate(-1)}
    />
    );
};

export default PageTitle;
