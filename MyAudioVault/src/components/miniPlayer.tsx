import { Pressable, StyleSheet, View } from 'react-native';
import { globalStyles } from "@/constants/globalStyles";
import { useAudioPlayerContext } from "@/contexts/audioPlayerContext";
import { ProgressBarForMiniPlayer } from './progressBar';
import { Colors } from '@/constants/colors';
import Feather from '@react-native-vector-icons/feather';
import { useEffect, useState } from 'react';
import { router, useSegments } from 'expo-router';

export const MiniPlayer: React.FC = () => {
    const { player, previous, next, play, stop, songDetails } = useAudioPlayerContext();
    const [isPlaying, setIsPlaying] = useState(false);

    useEffect(() => {
        const subscription = player.addListener('playbackStatusUpdate', (status) => {
            setIsPlaying(status.playing)
        })
        return () => {
            subscription.remove();
        };
    }, [player]);

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
                        style={({ pressed }) => [globalStyles.secondaryButton, pressed && globalStyles.pressed]}
                        accessibilityRole="button"
                        accessibilityLabel="Previous track"
                        onPress={previous}
                    >
                        <Feather name="skip-back" size={22} color={Colors.mainTheme.primary} />
                    </Pressable>

                    <Pressable
                        style={({ pressed }) => [globalStyles.playButton, pressed && globalStyles.pressed]}
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
                        style={({ pressed }) => [globalStyles.secondaryButton, pressed && globalStyles.pressed]}
                        accessibilityRole="button"
                        accessibilityLabel="Next track"
                        onPress={next}
                    >
                        <Feather name="skip-forward" size={22} color={Colors.mainTheme.primary} />
                    </Pressable>


                    <Pressable
                        style={({ pressed }) => [globalStyles.secondaryButton, pressed && globalStyles.pressed]}
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
        backgroundColor: Colors.mainTheme.surfaceRaised,
        flexDirection: 'row',
        borderWidth: 1,
        borderTopWidth: 2,
        borderColor: Colors.mainTheme.border,
        borderTopColor: Colors.mainTheme.primary,
        borderTopLeftRadius: 2,
        borderTopRightRadius: 2,
    },
    controls: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: 10,
        gap: 10
    },
});
