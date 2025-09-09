import { Button } from 'antd';
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { useLang } from '@/Language';

type Props = {
    collapsed: boolean;
    onToggle: (collapsed: boolean) => void;
}

const MenuToggle = ({ collapsed, onToggle }: Props) => {
    const { lang: { dir } } = useLang();
    const collapseIcon = dir === 'ltr' ? <MenuFoldOutlined /> : <MenuUnfoldOutlined />;
    const expandIcon = dir === 'ltr' ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />;

    return (
        <Button
            type="text"
            style={{ height: '60px', fontSize: '16px' }}
            icon={collapsed ? expandIcon : collapseIcon}
            onClick={() => onToggle(!collapsed)}
        />
    );
};

export default MenuToggle;
