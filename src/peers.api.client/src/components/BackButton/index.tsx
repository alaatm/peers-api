import { useLang } from '@/Language';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

const BackButton = () => {
    const { lang: { dir } } = useLang();

    return dir === 'ltr'
        ? <FontAwesomeIcon icon="arrow-left" />
        : <FontAwesomeIcon icon="arrow-right" />;
};

export default BackButton;
