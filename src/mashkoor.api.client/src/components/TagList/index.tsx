import EditableTag from './EditableTag';
import NewTag from './NewTag';
import './index.css';

type Props = {
    tags: string[];
    editing: boolean;
    disabled?: boolean;
    onChange: (tags: string[]) => void;
};

const TagList = ({ tags, editing, disabled, onChange }: Props) => {
    const onTagAdded = (value: string) => {
        if (value && tags.indexOf(value) === -1) {
            onChange([...tags, value]);
        }
    };

    const onTagEdited = (originalValue: string, newValue: string) => {
        const newTags = tags.map(t => t === originalValue ? newValue : t);
        onChange(newTags);
    };

    const onTagRemoved = (value: string) => {
        const newTags = tags.filter(t => t !== value);
        onChange(newTags);
    };

    return (
        <>
            {tags.map((t) => <EditableTag value={t} disabled={disabled} editing={editing} key={t} onTagEdited={onTagEdited} onTagRemoved={onTagRemoved} />)}
            <NewTag visible={editing} disabled={disabled} onTagAdded={onTagAdded} />
        </>
    )
};

export default TagList;
