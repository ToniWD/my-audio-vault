import { Stack, router, useSegments } from 'expo-router';
import { Pressable, View, StyleSheet } from 'react-native';
import Feather from '@react-native-vector-icons/feather';
import { Colors } from '@/constants/colors';
import { globalStyles, headerOptions } from '@/constants/globalStyles';
import { AudioPlayerProvider, useAudioPlayerContext } from '@/contexts/audioPlayerContext';
import { MiniPlayer } from '@/components/miniPlayer';
import { useEffect } from 'react';

export default function MainLayout() {
    return (
        <AudioPlayerProvider>
            <View style={styles.container}>
                <Stack
                    screenOptions={{
                        ...headerOptions,
                        headerTitle: '',

                        headerLeft: () => (
                            <Pressable
                                onPress={() => console.log('Open Menu')}
                                style={globalStyles.headerButtonLeft}
                                hitSlop={8}
                            >
                                <Feather name="menu" size={28} color={Colors.mainTheme.primary} />
                            </Pressable>
                        ),

                        headerRight: () => (
                            <Pressable
                                onPress={() => router.push('/profile')}
                                style={globalStyles.headerButtonRight}
                                hitSlop={8}
                            >
                                <Feather name="user" size={28} color={Colors.mainTheme.primary} />
                            </Pressable>
                        )
                    }}
                >
                    {/* `profile` is a nested Stack with its own header, so hide ours to
                avoid rendering two headers stacked on top of each other. */}
                    <Stack.Screen name="profile" options={{ headerShown: false }} />

                    <Stack.Screen
                        name="audioPlayer"
                        options={{
                            headerShown: true
                        }}
                    />

                </Stack>
                <MiniPlayer />
            </View>
        </AudioPlayerProvider>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.mainTheme.background,
    },
    stackContainer: {
        flex: 1,
    },
    miniPlayerWrapper: {
        position: 'absolute',
        bottom: 0,
        left: 0,
        right: 0,
        // Poți adăuga un shadow sau fundal dacă e nevoie
    },
});
