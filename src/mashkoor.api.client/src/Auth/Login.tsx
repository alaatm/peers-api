import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router'
import { useTranslation } from 'react-i18next';
import { Form, Input, Button, Checkbox, Alert, Card, Divider } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import useAuth from './useAuth';
import mashkoor from '@/assets/logo_head.png';
import './Login.css';

const Login = () => {
    const { t } = useTranslation('forms', { keyPrefix: 'login' });
    const { isAuthenticated, signin } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const [handled, setHandled] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>();

    useEffect(() => {
        if (isAuthenticated && !handled) {
            navigate('/', { replace: true });
        }
    }, [isAuthenticated, handled, navigate]);

    const state = location.state as { from: Location };
    const from = state ? state.from.pathname : '/';

    const onFinish = async (values: { username: string; password: string, remember: boolean }) => {
        setLoading(true);
        setError(null);

        if (await signin(values.username, values.password, values.remember)) {
            setHandled(true);
            setError(null);
            setLoading(false);
            navigate(from, { replace: true });
        } else {
            setError(t('invalidLogin'));
            setLoading(false);
        }
    };

    return (
        <div className="container">
            <div className="login-container">
                <img className="login-logo" src={mashkoor} alt="logo" width={358} />
                <Divider />

                {error && <Alert showIcon message={error} type="error" />}

                <Card className="login-card" title={t('title')}>
                    <Form
                        name="login"
                        initialValues={{ remember: true }}
                        onFinish={onFinish}
                        autoComplete="on"
                    >
                        <Form.Item
                            name="username"
                            rules={[
                                { required: true, message: t('username.required') },
                                { type: 'email', message: t('username.invalid') }]}
                        >
                            <Input prefix={<UserOutlined />} placeholder={t('username.placeholder')} />
                        </Form.Item>

                        <Form.Item
                            name="password"
                            rules={[{ required: true, message: t('password.required') }]}
                        >
                            <Input.Password prefix={<LockOutlined />} placeholder={t('password.placeholder')} />
                        </Form.Item>

                        <Form.Item>
                            <Form.Item name="remember" valuePropName="checked" noStyle>
                                <Checkbox>{t('rememberMe')}</Checkbox>
                            </Form.Item>

                            <Button className="reset" type="link">{t('forgotPassword')}</Button>
                            {/* <a className="reset" href="">
                                Forgot password?
                            </a> */}
                        </Form.Item>

                        <Form.Item>
                            <Button type="primary" htmlType="submit" loading={loading}>
                                {t('login')}
                            </Button>
                        </Form.Item>
                    </Form>
                </Card>
            </div>
        </div>
    );
};

export default Login;
