import { Image, Pressable, ScrollView, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';
import Feather from '@react-native-vector-icons/feather';
import { useAudioPlayerContext } from '@/contexts/audioPlayerContext';
import { ProgressBar } from '@/components/progressBar';
import { useEffect, useState } from 'react';
import { getApiSongs } from '@/api';
import { useMusicStore } from '@/hooks/useMusicStore';

const ARTWORK_URI = 'https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=800';

// Below this width the player stacks vertically (phone / narrow window).
const WIDE_BREAKPOINT = 820;

export default function AudioPlayerScreen() {
    // ------------------------------------------------------------ state
    const { width } = useWindowDimensions();
    const isWide = width >= WIDE_BREAKPOINT;
    const [isPlaying, setIsPlaying] = useState(false);

    // ------------------------------------------------- player & store
    const { player, songDetails, setSongDetails, play, stop } = useAudioPlayerContext();
    const setPlaylist = useMusicStore((state) => state.setPlaylist);
    const getNextSong = useMusicStore((state) => state.getNextSong);
    const getPrevSong = useMusicStore((state) => state.getPreviousSong);
    const setCurrentSongId = useMusicStore((state) => state.setCurrentSongId);
    const playlist = useMusicStore((state) => state.playlist);

    // ---------------------------------------------------------- handlers
    const handleInitialSongLoad = async () => {
        const result = await getApiSongs({
            query: {
                PageNumber: 0,
                PageSize: 10,
                SortBy: undefined,
                Search: undefined
            }
        });

        if (result.error) {
            console.error('Error fetching songs:', result.error);
            return;
        }

        if (result.data?.data) {
            setPlaylist(result.data?.data);
            await setSongDetails(result.data?.data[0]);
            setCurrentSongId(result.data?.data[0]?.id);
            setIsPlaying(player.playing);
            console.log('songDetails', songDetails);
            console.log('playlist', result.data?.data);
        }
    }

    const handlePrevPress = () => {
        if (player.currentTime > 3) {
            player.seekTo(0);
            return;
        }

        const prevSong = getPrevSong();
        if (prevSong) {
            setSongDetails(prevSong);
            setCurrentSongId(prevSong.id);
            play();
        }
        else {
            player.seekTo(0);
        }
    };

    const handleTogglePlayPress = () => {
        if (player.playing) {
            stop();
        } else {
            play();
        }
        setIsPlaying(player.playing);
    };

    const handleNextPress = () => {
        const nextSong = getNextSong();
        if (nextSong) {
            setSongDetails(nextSong);
            setCurrentSongId(nextSong.id);
            play();
        }
    };

    // ------------------------------------------------------------ effects
    useEffect(() => {
        handleInitialSongLoad();
    }, []);

    // ------------------------------------------------------------- render
    return (
        <View style={globalStyles.screen}>
            <ScrollView
                contentContainerStyle={styles.scrollContent}
                showsVerticalScrollIndicator={false}
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
                            <Text style={styles.artworkMetaText}>{songDetails?.availableQualities?.at(0)?.format || 'Unknown Format'}</Text>
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
                                    {songDetails?.releaseDate || 'No date available'}
                                </Text>
                            </View>

                            <Pressable
                                style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}
                                accessibilityRole="button"
                                accessibilityLabel="Add to favourites"
                            >
                                <Feather name="heart" size={20} color={Colors.mainTheme.primary} />
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
                                onPress={handlePrevPress}
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
                                onPress={handleNextPress}
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
                        <View style={styles.footerCard}>
                            <Pressable
                                style={({ pressed }) => [styles.footerAction, pressed && styles.pressed]}
                                accessibilityRole="button"
                                accessibilityLabel="Volume"
                            >
                                <Feather name="volume-2" size={18} color={Colors.mainTheme.textPrimary} />
                                <Text style={styles.footerLabel}>Volume</Text>
                            </Pressable>

                            <View style={styles.footerDivider} />

                            <Pressable
                                style={({ pressed }) => [styles.footerAction, pressed && styles.pressed]}
                                accessibilityRole="button"
                                accessibilityLabel="Open queue"
                            >
                                <Feather name="list" size={18} color={Colors.mainTheme.textPrimary} />
                                <Text style={styles.footerLabel}>Queue</Text>
                            </Pressable>
                        </View>
                    </View>
                </View>
            </ScrollView>
        </View>
    );
}

const styles = StyleSheet.create({
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
        maxWidth: 1000,
        flexDirection: 'row',
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
