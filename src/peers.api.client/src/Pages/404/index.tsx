import { Button, Result } from 'antd';
import { Link } from 'react-router';

const NotFound = () => (
    <Result
        status="404"
        title="404"
        subTitle="Sorry, the page you visited does not exist."
        extra={<Button type="primary"><Link replace to="/">Go to home page</Link></Button>}
    />
);

export default NotFound;
