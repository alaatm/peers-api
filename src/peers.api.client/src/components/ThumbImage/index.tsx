import { useState } from 'react';
import { Image, ImageProps, } from 'antd';

const ThumbImage = ({ src, ...rest }: ImageProps) => {
    const [visible, setVisible] = useState(false);

    const hasExtension = src!.split('/').pop()!.split('.').length > 1;
    const filenameWithoutExtension = src!.split('.').slice(0, -1).join('.');
    const extension = src!.split('.').pop();
    const thumbUrl = hasExtension ? `${filenameWithoutExtension}-thumb.${extension}` : `${src}-thumb`;

    return (
        <>
            <Image
                {...rest}
                preview={{ visible: false }}
                src={thumbUrl}
                onClick={() => setVisible(true)}
            />
            <div style={{ display: 'none' }}>
                <Image.PreviewGroup preview={{ visible, onVisibleChange: (vis) => setVisible(vis) }}>
                    <Image src={src} loading="lazy"></Image>
                </Image.PreviewGroup>
            </div>
        </>
    );
};

export default ThumbImage;
