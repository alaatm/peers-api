import { useEffect, useRef, useState } from 'react';
import { Input, type InputRef, Tag, Tooltip } from 'antd';

type Props = {
    value: string;
    editing: boolean;
    disabled?: boolean;
    onTagEdited: (originalValue: string, newValue: string) => void;
    onTagRemoved: (value: string) => void;
};

const EditableTag = ({ value, editing, disabled, onTagEdited, onTagRemoved }: Props) => {
    const [updateMode, setUpdateMode] = useState(false);
    const [inputValue, setInputValue] = useState(value);
    const inputRef = useRef<InputRef>(null);

    useEffect(() => {
        if (updateMode) {
            inputRef.current?.focus();
        }
    }, [updateMode])

    const handleInputConfirm = () => {
        onTagEdited(value, inputValue);
        setUpdateMode(false);
    };

    const isLongTag = value.length > 20;

    const tagElem = (
        <Tag
            className="edit-tag"
            closable={editing && !updateMode}
            onClose={() => onTagRemoved(value)}
        >
            <span
                onDoubleClick={() => {
                    if (editing && !updateMode && !disabled) {
                        setUpdateMode(true);
                        setInputValue(value);
                    }
                }}
            >
                {isLongTag ? `${value.slice(0, 20)}...` : value}
            </span>
        </Tag>
    );

    return updateMode ? (
        <Input
            disabled={disabled}
            ref={inputRef}
            size="small"
            className="tag-input"
            value={inputValue}
            onChange={e => setInputValue(e.target.value)}
            onBlur={handleInputConfirm}
            onPressEnter={handleInputConfirm}
        />
    ) : isLongTag ? (
        <Tooltip title={value}>
            {tagElem}
        </Tooltip>
    ) : (
        tagElem
    );
};

export default EditableTag;
