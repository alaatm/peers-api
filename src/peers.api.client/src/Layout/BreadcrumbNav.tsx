import { Breadcrumb } from 'antd';
import { Link } from 'react-router';
import { useTranslation } from 'react-i18next';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

const BreadcrumbNav = () => {
    const { t, i18n } = useTranslation();
    const path = window.location.pathname;
    const parts = path
        .split('/')
        // Remove empty parts
        .filter(p => !!p)
        // Filter out ids and numbers but not phone numbers which start with +
        .filter(p => p.startsWith('+') || isNaN(Number(p)))
        .map(p => {
            const decoded = decodeURIComponent(p);
            const key = toCamelCase(decoded.replace(/-/g, ' '));
            return i18n.exists(key) ? t(key) : decoded;
        });

    const items = [
        {
            title: <Link to="/"> <FontAwesomeIcon icon="home-lg" /></Link>,
        },
        ...parts.map((p, i) => ({
            title: <span className="force-ltr" key={`${p}_${i}`}>{p}</span>,
        })),
    ];

    return <Breadcrumb items={items} />;
};

export default BreadcrumbNav;

function toCamelCase(str: string): string {
    return str
        .split(' ')
        .map((word, index) => {
            if (index === 0) {
                return word.toLowerCase();
            }
            return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
        })
        .join('');
}
