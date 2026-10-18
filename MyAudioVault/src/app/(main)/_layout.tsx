import { Stack, router } from 'expo-router';
import { Pressable } from 'react-native';
import Feather from '@react-native-vector-icons/feather';
import { Colors } from '@/constants/colors';
import { globalStyles, headerOptions } from '@/constants/globalStyles';
import { AudioPlayerProvider } from '@/contexts/audioPlayerContext';

export default function MainLayout() {
    return (
        <AudioPlayerProvider>
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
        </AudioPlayerProvider>
    );
}
