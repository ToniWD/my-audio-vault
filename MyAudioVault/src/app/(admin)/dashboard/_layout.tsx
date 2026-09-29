import { Redirect, Stack, useRouter } from 'expo-router';
import { ActivityIndicator, Pressable, View } from 'react-native';
import { Colors } from '@/constants/colors';
import { globalStyles, headerOptions } from '@/constants/globalStyles';
import { useAuth } from '@/hooks/useAuth';
import Feather from '@react-native-vector-icons/feather';
import { useSafeBack } from '@/hooks/useSafeBack';

export default function AdminLayout() {
    const { user, isLoading } = useAuth();
    const { goBackOrFallback } = useSafeBack()

    if (isLoading) {
        return (
            <View style={[globalStyles.screen, globalStyles.center]}>
                <ActivityIndicator size="large" color={Colors.mainTheme.primary} />
            </View>
        );
    }

    if (!user || user.role !== 3) {
        return <Redirect href="/(main)" />;
    }

    return (
        <Stack
            screenOptions={{
                ...headerOptions,
                headerTitle: 'Back',

                headerLeft: () => (
                    <Pressable
                        onPress={() => goBackOrFallback("/profile")}
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
                options={{ title: 'Admin Dashboard' }}

            />
        </Stack>
    );
}
