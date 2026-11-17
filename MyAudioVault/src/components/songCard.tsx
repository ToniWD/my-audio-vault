import { SongDetailsDto } from "@/api";
import { Colors } from "@/constants/colors";
import { Fonts } from "@/constants/typography";
import { globalStyles } from "@/constants/globalStyles";
import { Pressable, StyleSheet, Text, View } from 'react-native';
import SongCover from './songCover';
import { formatDuration } from "@/utils/format";

const CARD_WIDTH = 200;
const CARD_HEIGHT = 280;
const ARTWORK_SIZE = CARD_WIDTH - 10;

export interface SongCardProps {
    songDetails: SongDetailsDto;
    /** Called when the card is pressed. Playback wiring is left to the consumer. */
    onPress?: (song: SongDetailsDto) => void;
}

/**
 * Fixed-size song item for use inside lists/grids.
 * Artwork sits on top with the song details underneath; long text is
 * ellipsized so nothing can spill past the card's boundaries.
 */
export default function SongCard({ songDetails, onPress }: SongCardProps) {
    return (
        <Pressable
            style={({ pressed }) => [
                globalStyles.cardList,
                styles.container,
                pressed && styles.containerPressed,
                pressed && globalStyles.pressed,
            ]}
            onPress={() => onPress?.(songDetails)}
            accessibilityRole="button"
            accessibilityLabel={`Play ${songDetails.title || 'Unknown Title'}`}
        >
            <View style={styles.artworkFrame}>
                <SongCover songId={songDetails.id} style={styles.artwork} />
            </View>

            <View style={styles.info}>
                <Text style={styles.title} numberOfLines={1}>
                    {songDetails.title || 'Unknown Title'}
                </Text>

                <Text style={styles.meta} numberOfLines={1}>
                    {songDetails.genre || 'Unknown Genre'}
                </Text>

                <Text style={styles.duration} numberOfLines={1}>
                    {formatDuration(songDetails.durationSeconds)}
                </Text>
            </View>
        </Pressable>
    );
}

const styles = StyleSheet.create({
    container: {
        width: CARD_WIDTH,
        height: CARD_HEIGHT,
        padding: 5,
        gap: 10,
    },
    containerPressed: {
        borderColor: Colors.mainTheme.borderActive,
    },
    artworkFrame: {
        width: ARTWORK_SIZE,
        height: ARTWORK_SIZE,
        borderRadius: 2,
        overflow: 'hidden',
        backgroundColor: Colors.mainTheme.background,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },
    artwork: {
        width: '100%',
        height: '100%',
    },
    info: {
        width: '100%',
        minWidth: 0,
        gap: 4,
    },
    title: {
        color: Colors.mainTheme.textPrimary,
        fontSize: 14,
        letterSpacing: 0.4,
        fontFamily: Fonts.monoBold,
    },
    meta: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 11,
        letterSpacing: 0.4,
        fontFamily: Fonts.monoRegular,
        textTransform: 'uppercase',
    },
    duration: {
        color: Colors.mainTheme.textMuted,
        fontSize: 11,
        letterSpacing: 1,
        fontFamily: Fonts.monoRegular,
    },
});
