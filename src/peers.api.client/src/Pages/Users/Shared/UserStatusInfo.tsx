import { UserStatus } from '@/api';
import { FontAwesomeIcon, type FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import { Space } from 'antd';

type Props = {
    userStatus: UserStatus;
    iconSize?: FontAwesomeIconProps['size'];
};

const UserStatusInfo = ({ userStatus, iconSize }: Props) => {
    const statusIcon = userStatus === UserStatus.Suspended
        ? <FontAwesomeIcon icon="ban" color="#ff4d4f" size={iconSize} />
        : userStatus === UserStatus.Banned
            ? <FontAwesomeIcon icon="user-slash" color="#ff4d4f" size={iconSize} />
            : null;

    return statusIcon
        ? <Space>{statusIcon}{UserStatus[userStatus]}</Space>
        : null;
};

export default UserStatusInfo;
