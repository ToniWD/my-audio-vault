import { Stack, router } from 'expo-router';
import { Pressable } from 'react-native';
import Feather from '@react-native-vector-icons/feather';
import { Colors } from '@/constants/colors';
import { globalStyles, headerOptions } from '@/constants/globalStyles';
import { useSafeBack } from '@/hooks/useSafeBack';


export default function ProfileLayout() {
    const { goBackOrFallback } = useSafeBack();

    return (
        <Stack
            screenOptions={{
                ...headerOptions,
                headerTitle: 'My Profile',

                headerLeft: () => (
                    <Pressable
                        onPress={() => goBackOrFallback("/(main)")}
                        style={globalStyles.headerButtonLeft}
                        hitSlop={8}
                    >
                        <Feather name="arrow-left" size={28} color={Colors.mainTheme.primary} />
                    </Pressable>
                ),
            }}
        >
            <Stack.Screen
                name="index"
                options={{ title: 'Profile' }}
            />
            <Stack.Screen
                name="edit"
                options={{
                    title: 'Edit Profile',
                    headerBackTitle: 'Back',
                    presentation: 'modal'
                }}
            />
            <Stack.Screen
                name="password"
                options={{
                    title: 'Change Password',
                    headerBackTitle: 'Back',
                    presentation: 'modal'
                }}
            />
        </Stack>
    );
}
