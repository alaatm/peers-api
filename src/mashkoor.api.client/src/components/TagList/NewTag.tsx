import { useEffect, useRef, useState } from 'react';
import { Input, InputRef, Tag } from 'antd';
import { PlusOutlined } from '@ant-design/icons';

type Props = {
    visible: boolean;
    disabled?: boolean;
    onTagAdded: (value: string) => void;
};

const NewTag = ({ visible, disabled, onTagAdded }: Props) => {
    const [inputVisible, setInputVisible] = useState(false);
    const [inputValue, setInputValue] = useState('');
    const inputRef = useRef<InputRef>(null);

    useEffect(() => {
        if (inputVisible) {
            inputRef.current?.focus();
        }
    }, [inputVisible])

    const handleNewInputConfirm = () => {
        onTagAdded(inputValue);
        setInputVisible(false);
        setInputValue('');
    };

    return visible ? (
        <>
            {inputVisible && (
                <Input
                    disabled={disabled}
                    ref={inputRef}
                    size="small"
                    className="tag-input"
                    value={inputValue}
                    onChange={e => setInputValue(e.target.value)}
                    onBlur={handleNewInputConfirm}
                    onPressEnter={handleNewInputConfirm}
                />
            )}
            {!inputVisible && (
                <Tag className="new-tag" onClick={() => setInputVisible(!disabled)}>
                    <PlusOutlined disabled={disabled} /> New Tag
                </Tag>
            )}
        </>
    ) : null;
};

export default NewTag;
