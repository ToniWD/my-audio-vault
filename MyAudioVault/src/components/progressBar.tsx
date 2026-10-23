import { Colors } from "@/constants/colors";
import { Fonts } from "@/constants/typography";
import { AudioPlayer } from "expo-audio";
import { useEffect, useState } from "react";
import { View, StyleSheet, Text, StyleProp, ViewStyle } from "react-native";
import Slider from '@react-native-community/slider';
import { SongDetailsDto } from "@/api";

interface ProgressBarProps {
    player: AudioPlayer;
    viewStyle?: StyleProp<ViewStyle> | null; // Semnul '?' o face opțională la apelare
}

export const ProgressBar: React.FC<ProgressBarProps> = ({ player, viewStyle = styles.progressCard }) => {
    const [progress, setProgress] = useState<number>(0);
    const [duration, setDuration] = useState<number>(0);
    const [currentTime, setCurrentTime] = useState<number>(0);

    const [changingProgress, setChangingProgress] = useState<boolean>(false);
    const [sliderValue, setSliderValue] = useState<number>(0);


    useEffect(() => {
        const subscription = player.addListener("playbackStatusUpdate", async (status) => {
            setCurrentTime(status.currentTime);
            setDuration(status.duration);
            if (status.duration > 0) {
                setProgress((status.currentTime / status.duration) * 100);
            }
        });

        return () => {
            subscription.remove();
        };
    }, [player]);

    useEffect(() => {
        if (!changingProgress) {
            setSliderValue(progress);
        }
    }, [progress]);

    return (
        <View style={viewStyle}>
            <Slider
                value={sliderValue}
                onValueChange={(value) => {
                    setChangingProgress(true);
                    setSliderValue(value);
                }}
                onSlidingComplete={async (value) => {
                    if (progress !== value) {
                        const seekTime = Math.floor((value / 100) * duration);
                        await player.seekTo(seekTime);
                    }
                    setChangingProgress(false);

                    setTimeout(() => {
                        setChangingProgress(false);
                    }, 300);
                }}
                lowerLimit={0}
                upperLimit={100}
                minimumValue={0}
                maximumValue={100}
                thumbTintColor={Colors.mainTheme.primary}
            />

            <View style={styles.timeRow}>
                <Text style={styles.time}>{String(Math.floor(currentTime / 60)).padStart(2, '0')}:{String(Math.floor(currentTime % 60)).padStart(2, '0')}</Text>
                <Text style={styles.time}>{String(Math.floor(duration / 60)).padStart(2, '0')}:{String(Math.floor(duration % 60)).padStart(2, '0')}</Text>
            </View>
        </View>
    );
}

export const ProgressBarForMiniPlayer: React.FC<{ player: AudioPlayer, songDetails: SongDetailsDto | null }> = ({ player, songDetails }) => {
    const [progress, setProgress] = useState<number>(0);
    const [duration, setDuration] = useState<number>(0);
    const [currentTime, setCurrentTime] = useState<number>(0);

    const [changingProgress, setChangingProgress] = useState<boolean>(false);
    const [sliderValue, setSliderValue] = useState<number>(0);

    useEffect(() => {
        const subscription = player.addListener("playbackStatusUpdate", async (status) => {
            setCurrentTime(status.currentTime);
            setDuration(status.duration);
            if (status.duration > 0) {
                setProgress((status.currentTime / status.duration) * 100);
            }
        });

        return () => {
            subscription.remove();
        };
    }, [player]);

    useEffect(() => {
        if (!changingProgress) {
            setSliderValue(progress);
        }
    }, [progress]);

    return (
        <View style={[styles.container, { gap: 5 }]}>
            {songDetails && <Text style={[styles.time, { color: Colors.mainTheme.primary }]}>{songDetails.title}</Text>}
            <Slider
                value={sliderValue}
                onValueChange={(value) => {
                    setChangingProgress(true);
                    setSliderValue(value);
                }}
                onSlidingComplete={async (value) => {
                    if (progress !== value) {
                        const seekTime = Math.floor((value / 100) * duration);
                        await player.seekTo(seekTime);
                    }
                    setChangingProgress(false);

                    setTimeout(() => {
                        setChangingProgress(false);
                    }, 300);
                }}
                lowerLimit={0}
                upperLimit={100}
                minimumValue={0}
                maximumValue={100}
                thumbTintColor={Colors.mainTheme.primary}
            />

            <View style={[styles.timeRow, { marginTop: 0 }]}>
                <Text style={styles.time}>{String(Math.floor(currentTime / 60)).padStart(2, '0')}:{String(Math.floor(currentTime % 60)).padStart(2, '0')}</Text>
                <Text style={styles.time}>{String(Math.floor(duration / 60)).padStart(2, '0')}:{String(Math.floor(duration % 60)).padStart(2, '0')}</Text>
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        paddingInline: 10
    },
    progressCard: {
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
        padding: 20,
        marginTop: 24,
    },
    progressTrack: {
        height: 6,
        borderRadius: 3,
        backgroundColor: Colors.mainTheme.border,
        justifyContent: 'center',
    },
    progressFill: {
        width: '35%',
        height: 6,
        borderRadius: 3,
        backgroundColor: Colors.mainTheme.primary,
    },
    progressKnob: {
        position: 'absolute',
        left: '35%',
        width: 14,
        height: 14,
        marginLeft: -7,
        borderRadius: 7,
        backgroundColor: Colors.mainTheme.primary,
        borderWidth: 2,
        borderColor: Colors.mainTheme.background,
    },
    timeRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginTop: 10,
    },
    time: {
        color: Colors.mainTheme.textSecondary,
        fontSize: 12,
        letterSpacing: 0.5,
        fontFamily: Fonts.monoRegular,
    },
});