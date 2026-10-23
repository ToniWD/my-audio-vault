import { usePresignedUrlCache } from "@/hooks/useImagePresignedUrlStore";
import { useEffect, useState } from "react";
import { Image, ImageStyle, StyleProp } from 'react-native';

export default function SongCover({ songId, style }: { songId: string | null, style?: StyleProp<ImageStyle> }) {
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [hasError, setHasError] = useState(false);
    const getCoverImageUrl = usePresignedUrlCache((state) => state.getCoverImageUrl);

    useEffect(() => {
        if (songId) {
            let isMounted = true;
            getCoverImageUrl(songId).then((url) => {
                if (isMounted) setImageUrl(url);
            });
            return () => { isMounted = false; };
        }
    }, [songId, getCoverImageUrl]);

    const sourceToDisplay = (imageUrl && !hasError)
        ? { uri: imageUrl }
        : require('@/assets/images/default-cover.jpeg');

    return (
        <Image
            source={sourceToDisplay}
            style={style}
            resizeMode="cover"
            accessibilityLabel="Album artwork"
            onError={() => setHasError(true)}
        />
    );
}