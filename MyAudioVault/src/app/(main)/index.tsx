import { View } from 'react-native';
import { globalStyles } from '@/constants/globalStyles';
import SongGrid from '@/components/songGrid';
import { useAudioPlayerContext } from '@/contexts/audioPlayerContext';

export default function MainIndex() {
    const { generatePlaylistStartingWithSong } = useAudioPlayerContext();

    return (
        <View style={globalStyles.screen}>
            <SongGrid
                onSongPress={(song) => {
                    void generatePlaylistStartingWithSong(song);
                }}
            />
        </View>
    );
}
