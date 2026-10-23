import { Image, Pressable, ScrollView, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { globalStyles } from "@/constants/globalStyles";
import { useAudioPlayerContext } from "@/contexts/audioPlayerContext";
import { ProgressBar, ProgressBarForMiniPlayer } from './progressBar';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';
import Feather from '@react-native-vector-icons/feather';
import { useEffect, useState } from 'react';
import { router, useSegments } from 'expo-router';

const WIDE_BREAKPOINT = 1080;

export const MiniPlayer: React.FC = () => {
    const { player, previous, next, play, stop, songDetails } = useAudioPlayerContext();
    const [isPlaying, setIsPlaying] = useState(false);

    useEffect(() => {
        setIsPlaying(player.playing);
    }, []);

    const handleTogglePlayPress = () => {
        if (player.playing) {
            stop();
        } else {
            play();
        }
        setIsPlaying(player.playing);
    };

    const segments = useSegments() as string[];

    const isAudioPlayerActive = segments.includes('audioPlayer');

    const showMiniPlayer = songDetails && !isAudioPlayerActive;

    return (
        (showMiniPlayer) ? (
            <View style={styles.card}>
                <ProgressBarForMiniPlayer player={player} songDetails={songDetails} />

                {/* -------------------------------------------- controls */}
                <View style={styles.controls}>
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
                        onPress={() => router.push('/audioPlayer')}
                        accessibilityRole="button"
                        accessibilityLabel="Open audio player"
                    >
                        <Feather name="chevron-up" size={20} color={Colors.mainTheme.primary} />
                    </Pressable>
                </View>
            </View>
        ) : (
            null
        )
    );
}

const styles = StyleSheet.create({
    card: {
        backgroundColor: Colors.mainTheme.surface,
        flexDirection: 'row',
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderTopLeftRadius: 16,
        borderTopRightRadius: 16,
        // gap: 5,
        // padding: 20,
        // marginTop: 24,
    },
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
        // marginTop: 24,
        padding: 10
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