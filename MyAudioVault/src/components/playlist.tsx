import { SongDetailsDto } from '@/api/types.gen';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';
import { globalStyles } from '@/constants/globalStyles';
import { useMusicStore } from '@/hooks/useMusicStore';
import Feather from '@react-native-vector-icons/feather';
import { FlatList, Image, Pressable, StyleProp, StyleSheet, Text, View, ViewStyle } from 'react-native';
import { useAudioPlayerContext } from '@/contexts/audioPlayerContext';

const FALLBACK_ARTWORK =
    'https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=800';

/**
 * Formats a duration coming from the API (`number | string`) as `mm:ss`.
 */
const formatDuration = (duration: number | string | undefined): string => {
    const total = Number(duration ?? 0);

    if (!Number.isFinite(total) || total <= 0) {
        return '--:--';
    }

    const minutes = Math.floor(total / 60);
    const seconds = Math.floor(total % 60);

    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
};

export interface PlaylistProps {
    /** Songs rendered as a scrollable list. */
    songsDetails: SongDetailsDto[];
    /**
     * Called when a song row is pressed.
     * Left optional on purpose: playback wiring is not implemented yet.
     */
    onSongPress?: (song: SongDetailsDto, index: number) => void;
    /** Overrides the highlighted row (defaults to the store's current song). */
    currentSongId?: string | null;
    viewStyle?: StyleProp<ViewStyle> | null;
}

/**
 * Scrollable list of songs, styled after the audio player screen
 * (terminal surfaces, mono type, neon accent on the active track).
 *
 * Render it inside `globalStyles.screen` to get the same padding as the
 * audio player screen.
 */
export const Playlist: React.FC<PlaylistProps> = ({
    songsDetails,
    onSongPress,
    currentSongId,
    viewStyle,
}) => {
    const storeCurrentSongId = useMusicStore((state) => state.currentSongId);
    const activeSongId = currentSongId ?? storeCurrentSongId;

    const renderSong = ({ item, index }: { item: SongDetailsDto; index: number }) => {
        const isActive = item.id === activeSongId;
        const quality = item.availableQualities?.at(0);

        return (
            <Pressable
                style={({ pressed }) => [
                    styles.songCard,
                    isActive && styles.songCardActive,
                    pressed && styles.pressed,
                ]}
                onPress={() => {
                    console.log(`Song pressed: ${item.title} (index: ${index}) ${onSongPress ? 'with onSongPress callback' : 'without onSongPress callback'}`);
                    onSongPress?.(item, index)
                }}
                accessibilityRole="button"
                accessibilityLabel={`Play ${item.title}`}
                accessibilityState={{ selected: isActive }}
            >
                {/* ------------------------------------------------ artwork */}
                <View style={styles.artworkFrame}>
                    <Image
                        source={{ uri: item.coverImageUrl || FALLBACK_ARTWORK }}
                        style={styles.artwork}
                        resizeMode="cover"
                        accessibilityLabel={`${item.title} cover`}
                    />
                </View>

                {/* -------------------------------------------------- details */}
                <View style={styles.songInfo}>
                    <View style={styles.songTitleRow}>
                        <Text
                            style={[styles.songTitle, isActive && styles.songTitleActive]}
                            numberOfLines={1}
                        >
                            {item.title || 'Unknown Title'}
                        </Text>

                        {item.isExplicit && (
                            <View style={styles.explicitBadge}>
                                <Text style={styles.explicitText}>E</Text>
                            </View>
                        )}
                    </View>

                    <Text style={styles.songMeta} numberOfLines={1}>
                        {item.genre || 'Unknown Genre'}
                    </Text>

                    <View style={styles.songFooter}>
                        <Text style={styles.songTag}>
                            {quality?.format?.toUpperCase() || 'AUDIO'}
                        </Text>
                        <Text style={styles.songMeta} numberOfLines={1}>
                            {item.releaseDate || 'No date available'}
                        </Text>
                    </View>
                </View>

                {/* --------------------------------------------------- aside */}
                <View style={styles.songAside}>
                    <Text style={styles.duration}>
                        {formatDuration(item.durationSeconds)}
                    </Text>

                    {isActive ? (
                        <Feather name="volume-2" size={16} color={Colors.mainTheme.primary} />
                    ) : (
                        <Feather name="play" size={16} color={Colors.mainTheme.textSecondary} />
                    )}
                </View>
            </Pressable>
        );
    };

    const listHeader = (
        <View style={styles.header}>
            <View style={globalStyles.flex1}>
                <Text style={styles.eyebrow}>PLAYLIST</Text>
                <Text style={styles.playlistTitle}>Up Next</Text>
                <Text style={styles.playlistSubtitle}>
                    {songsDetails.length} {songsDetails.length === 1 ? 'track' : 'tracks'}
                </Text>
            </View>
        </View>
    );

    const listEmpty = (
        <View style={styles.emptyCard}>
            <Feather name="music" size={28} color={Colors.mainTheme.textMuted} />
            <Text style={styles.emptyTitle}>No songs yet</Text>
            <Text style={styles.emptyText}>
                Songs added to the playlist will show up here.
            </Text>
        </View>
    );

    return (
        <View style={[styles.container, viewStyle]}>
            <FlatList
                data={songsDetails}
                renderItem={renderSong}
                keyExtractor={(item) => item.id}
                ListHeaderComponent={listHeader}
                ListEmptyComponent={listEmpty}
                contentContainerStyle={styles.listContent}
                showsVerticalScrollIndicator={false}
                style={{ flex: 1 }}
            />
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        // minWidth: 360,
        // maxWidth: 440,
        width: '100%',
        height: '100%',
        alignSelf: 'center',
    },

    // ------------------------------------------------------------------ header
    header: {
        flexDirection: 'row',
        alignItems: 'flex-start',
        gap: 16,
        marginBottom: 20,
    },
    eyebrow: {
        color: Colors.mainTheme.textMuted,
        fontSize: 11,
        fontWeight: '700',
        letterSpacing: 2.5,
        fontFamily: Fonts.monoBold,
        marginBottom: 8,
    },
    playlistTitle: {
        fontSize: 30,
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoRegular,
    },
    playlistSubtitle: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 14,
        fontFamily: Fonts.monoRegular,
        marginTop: 6,
    },
    iconButton: {
        width: 44,
        height: 44,
        borderRadius: 22,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },

    // -------------------------------------------------------------------- list
    listContent: {
        flexGrow: 1,
        paddingVertical: 16,
    },
    songCard: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 14,
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
        padding: 12,
        marginBottom: 12,
    },
    songCardActive: {
        borderColor: Colors.mainTheme.borderActive,
        backgroundColor: Colors.mainTheme.primaryBg,
    },
    pressed: {
        opacity: 0.7,
    },

    // ---------------------------------------------------------------- artwork
    artworkFrame: {
        width: 56,
        height: 56,
        borderRadius: 12,
        padding: 4,
        backgroundColor: Colors.mainTheme.background,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },
    artwork: {
        flex: 1,
        width: '100%',
        borderRadius: 8,
        backgroundColor: Colors.mainTheme.background,
    },

    // ---------------------------------------------------------------- details
    songInfo: {
        flex: 1,
        minWidth: 0,
        gap: 4,
    },
    songTitleRow: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 8,
    },
    songTitle: {
        flexShrink: 1,
        color: Colors.mainTheme.textPrimary,
        fontSize: 16,
        fontFamily: Fonts.monoBold,
    },
    songTitleActive: {
        color: Colors.mainTheme.primary,
    },
    songMeta: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 12,
        fontFamily: Fonts.monoRegular,
    },
    songFooter: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 10,
    },
    songTag: {
        color: Colors.mainTheme.textMuted,
        fontSize: 10,
        letterSpacing: 1.5,
        fontFamily: Fonts.monoBold,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 6,
        paddingHorizontal: 6,
        paddingVertical: 2,
        overflow: 'hidden',
    },
    explicitBadge: {
        width: 16,
        height: 16,
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.border,
    },
    explicitText: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 9,
        fontFamily: Fonts.monoBold,
    },

    // ------------------------------------------------------------------ aside
    songAside: {
        alignItems: 'flex-end',
        gap: 8,
    },
    duration: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 12,
        letterSpacing: 0.5,
        fontFamily: Fonts.monoRegular,
    },

    // ------------------------------------------------------------------ empty
    emptyCard: {
        alignItems: 'center',
        gap: 8,
        paddingVertical: 40,
        paddingHorizontal: 24,
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
    },
    emptyTitle: {
        color: Colors.mainTheme.textPrimary,
        fontSize: 16,
        fontFamily: Fonts.monoBold,
    },
    emptyText: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 13,
        fontFamily: Fonts.monoRegular,
        textAlign: 'center',
    },
});
