import { Image, Pressable, ScrollView, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';
import Feather from '@react-native-vector-icons/feather';
import { useAudioPlayerContext } from '@/contexts/audioPlayerContext';
import { ProgressBar } from '@/components/progressBar';
import { useEffect, useState } from 'react';
import { SongDetailsDto } from '@/api';
import { Playlist } from '@/components/playlist';
import { useSafeBack } from '@/hooks/useSafeBack';
import { router } from 'expo-router';

const ARTWORK_URI = 'https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=800';

// Below this width the player stacks vertically / shows toggle (phone / narrow window).
const WIDE_BREAKPOINT = 1080;

type ViewMode = 'player' | 'playlist';

export default function AudioPlayerScreen() {
    // ------------------------------------------------------------ state
    const { width } = useWindowDimensions();
    const isWide = width >= WIDE_BREAKPOINT;
    const [isPlaying, setIsPlaying] = useState(false);
    const [viewMode, setViewMode] = useState<ViewMode>('player');

    // ------------------------------------------------- player & more
    const { player, songDetails, playlist, play, stop, next, previous, playFromCurrentPlaylist, generatePlaylist } = useAudioPlayerContext();

    // ---------------------------------------------------------- handlers

    const handleTogglePlayPress = () => {
        if (player.playing) {
            stop();
        } else {
            play();
        }
        setIsPlaying(player.playing);
    };

    const toggleView = () => {
        setViewMode((current) => (current === 'player' ? 'playlist' : 'player'));
    };

    const handleSongPress = async (song: SongDetailsDto, index: number) => {
        playFromCurrentPlaylist(index)
    }

    // ------------------------------------------------------------ effects
    useEffect(() => {
        setIsPlaying(player.playing);
    }, []);

    // Switch to player view when entering wide layout
    useEffect(() => {
        if (isWide) {
            setViewMode('player');
        }
    }, [isWide]);

    // ------------------------------------------------------------- render
    const renderPlayer = () => (
        <ScrollView
            contentContainerStyle={styles.scrollContent}
            showsVerticalScrollIndicator={false}
            style={isWide ? styles.playerScroll : undefined}
        >
            <View style={[styles.layout, isWide && styles.layoutWide]}>
                {/* ------------------------------------------------ artwork */}
                <View style={[styles.artworkColumn, isWide && styles.artworkColumnWide]}>
                    <View style={styles.artworkFrame}>
                        <Image
                            source={{ uri: songDetails?.coverImageUrl || ARTWORK_URI }}
                            style={styles.artwork}
                            resizeMode="cover"
                            accessibilityLabel="Album artwork"
                        />
                    </View>

                    <View style={styles.artworkMeta}>
                        <Feather name="music" size={13} color={Colors.mainTheme.textMuted} />
                        <Text style={styles.artworkMetaText}>{songDetails?.availableQualities?.at(0)?.format?.toUpperCase() || 'UNKNOWN'}</Text>
                    </View>
                </View>

                {/* ------------------------------------------------ details */}
                <View style={[styles.detailsColumn, isWide && styles.detailsColumnWide]}>
                    <View style={styles.trackHeader}>
                        <View style={globalStyles.flex1}>
                            <Text style={styles.eyebrow}>NOW PLAYING</Text>
                            <Text style={[globalStyles.title, styles.trackTitle]} numberOfLines={1}>
                                {songDetails?.title || 'Unknown Title'}
                            </Text>
                            <Text style={styles.trackArtist} numberOfLines={1}>
                                {songDetails?.genre || songDetails?.releaseDate || 'No info available'}
                            </Text>
                        </View>

                        <Pressable
                            onPress={() => router.push('/(main)')}
                            accessibilityRole="button"
                            accessibilityLabel="go back"
                        >
                            <Feather name="chevron-down" size={20} color={Colors.mainTheme.primary} />
                        </Pressable>
                    </View>

                    {/* -------------------------------------------- progress */}
                    <ProgressBar player={player} />

                    {/* -------------------------------------------- controls */}
                    <View style={styles.controls}>
                        <Pressable
                            style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Shuffle"
                        >
                            <Feather name="shuffle" size={20} color={Colors.mainTheme.primary} />
                        </Pressable>

                        <Pressable
                            style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Previous track"
                            onPress={previous}
                        >
                            <Feather name="skip-back" size={22} color={Colors.mainTheme.primary} />
                        </Pressable>

                        <Pressable
                            style={({ pressed }) => [styles.playButton, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Play"
                            onPress={handleTogglePlayPress}
                        >
                            {isPlaying ? (
                                <Feather name="pause" size={28} color={Colors.mainTheme.background} />
                            ) : (
                                <Feather name="play" size={28} color={Colors.mainTheme.background} />
                            )}
                        </Pressable>

                        <Pressable
                            style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Next track"
                            onPress={next}
                        >
                            <Feather name="skip-forward" size={22} color={Colors.mainTheme.primary} />
                        </Pressable>

                        <Pressable
                            style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Repeat"
                        >
                            <Feather name="repeat" size={20} color={Colors.mainTheme.primary} />
                        </Pressable>
                    </View>

                    {/* ---------------------------------------------- footer */}
                    {!isWide &&
                        <View style={styles.footerCard}>
                            {/* <Pressable
                            style={({ pressed }) => [styles.footerAction, pressed && styles.pressed]}
                            accessibilityRole="button"
                            accessibilityLabel="Volume"
                        >
                            <Feather name="volume-2" size={18} color={Colors.mainTheme.textPrimary} />
                            <Text style={styles.footerLabel}>Volume</Text>
                        </Pressable>

                        <View style={styles.footerDivider} /> */}

                            <Pressable
                                style={({ pressed }) => [styles.footerAction, pressed && styles.pressed]}
                                accessibilityRole="button"
                                accessibilityLabel="Open queue"
                                onPress={!isWide ? toggleView : undefined}
                            >
                                <Feather name="list" size={18} color={Colors.mainTheme.textPrimary} />
                                <Text style={styles.footerLabel}>Queue</Text>
                                <Text style={styles.footerCount}>{playlist.length}</Text>
                            </Pressable>
                        </View>
                    }
                </View>
            </View>
        </ScrollView>
    );

    return (
        <View style={[globalStyles.screen, isWide && styles.wideScreen]}>
            {isWide ? (
                <View style={styles.wideContainer}>
                    <View style={styles.playerColumn}>
                        {renderPlayer()}
                    </View>
                    <View style={styles.playlistColumn}>
                        <Playlist songsDetails={playlist} onSongPress={handleSongPress} />
                    </View>
                </View>
            ) : (
                <View style={styles.narrowContainer}>
                    {!isWide && viewMode === 'playlist' && (
                        <View style={styles.toggleHeader}>
                            <Pressable
                                style={({ pressed }) => [styles.toggleButton, pressed && styles.pressed]}
                                onPress={toggleView}
                                accessibilityRole="button"
                                accessibilityLabel="Back to player"
                            >
                                <Feather name="arrow-left" size={18} color={Colors.mainTheme.primary} />
                                <Text style={styles.toggleText}>Back to Player</Text>
                            </Pressable>
                        </View>
                    )}

                    <View style={styles.viewContainer}>
                        {viewMode === 'player' ? renderPlayer() : <Playlist
                            songsDetails={playlist}
                            onSongPress={handleSongPress} />
                        }
                    </View>
                </View>
            )}
        </View>
    );
}

const styles = StyleSheet.create({
    wideScreen: {
        paddingHorizontal: 32,
    },
    wideContainer: {
        flex: 1,
        flexDirection: 'row',
        gap: 32,
        alignItems: 'stretch',
    },
    playerColumn: {
        flex: 1,
        minWidth: 0,
    },
    playlistColumn: {
        width: 420,
        flexShrink: 0,
        height: '100%',
    },
    playerScroll: {
        flex: 1,
    },

    narrowContainer: {
        flex: 1,
    },
    toggleHeader: {
        paddingTop: 4,
        paddingBottom: 12,
    },
    toggleButton: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 10,
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 12,
        paddingVertical: 12,
        paddingHorizontal: 16,
    },
    toggleText: {
        color: Colors.mainTheme.textPrimary,
        fontSize: 14,
        fontFamily: Fonts.monoBold,
    },
    toggleBadge: {
        backgroundColor: Colors.mainTheme.primaryBg,
        borderRadius: 10,
        paddingHorizontal: 8,
        paddingVertical: 2,
        borderWidth: 1,
        borderColor: Colors.mainTheme.borderActive,
    },
    toggleBadgeText: {
        color: Colors.mainTheme.primary,
        fontSize: 11,
        fontFamily: Fonts.monoBold,
    },
    viewContainer: {
        flex: 1,
    },
    footerCount: {
        color: Colors.mainTheme.textMuted,
        fontSize: 12,
        fontFamily: Fonts.monoRegular,
    },

    scrollContent: {
        flexGrow: 1,
        justifyContent: 'center',
        paddingVertical: 16,
    },

    // ------------------------------------------------------------------ layout
    layout: {
        width: '100%',
        maxWidth: 480,
        alignSelf: 'center',
        gap: 24,
    },
    layoutWide: {
        maxWidth: 920,
        flexDirection: 'column',
        alignItems: 'center',
        gap: 48,
        paddingVertical: 24,
    },

    // ----------------------------------------------------------------- artwork
    artworkColumn: {
        width: '100%',
        alignSelf: 'center',
        maxWidth: 360,
    },
    artworkColumnWide: {
        width: 340,
        flexShrink: 0,
    },
    artworkFrame: {
        width: '100%',
        aspectRatio: 1,
        padding: 10,
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 20,
    },
    artwork: {
        flex: 1,
        width: '100%',
        borderRadius: 12,
        backgroundColor: Colors.mainTheme.background,
    },
    artworkMeta: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 8,
        marginTop: 12,
    },
    artworkMetaText: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 12,
        letterSpacing: 1.5,
        fontFamily: Fonts.monoRegular,
    },

    // ----------------------------------------------------------------- details
    detailsColumn: {
        width: '100%',
        flex: 1,
    },
    detailsColumnWide: {
        minWidth: 0,
    },
    trackHeader: {
        flexDirection: 'row',
        alignItems: 'flex-start',
        gap: 16,
    },
    eyebrow: {
        color: Colors.mainTheme.textMuted,
        fontSize: 11,
        fontWeight: '700',
        letterSpacing: 2.5,
        fontFamily: Fonts.monoBold,
        marginBottom: 8,
    },
    trackTitle: {
        fontSize: 30,
        marginBottom: 0,
    },
    trackArtist: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 16,
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

    // ---------------------------------------------------------------- controls
    controls: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        marginTop: 24,
    },
    secondaryButton: {
        width: 48,
        height: 48,
        borderRadius: 24,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },
    playButton: {
        width: 72,
        height: 72,
        borderRadius: 36,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.primary,
        shadowColor: Colors.mainTheme.primary,
        shadowOffset: { width: 0, height: 0 },
        shadowOpacity: 0.35,
        shadowRadius: 14,
        elevation: 6,
    },
    pressed: {
        opacity: 0.7,
    },

    // ------------------------------------------------------------------ footer
    footerCard: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
        paddingHorizontal: 8,
        marginTop: 24,
    },
    footerAction: {
        flex: 1,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 10,
        paddingVertical: 16,
    },
    footerDivider: {
        width: 1,
        height: 28,
        backgroundColor: Colors.mainTheme.border,
    },
    footerLabel: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 13,
        fontFamily: Fonts.monoRegular,
    },
});
