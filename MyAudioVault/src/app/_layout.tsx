import { Stack, SplashScreen } from 'expo-router';
import { ActivityIndicator, View } from 'react-native';
import { AuthProvider, useAuthContext } from '../contexts/AuthContext';
import { useProtectedRoute } from '../hooks/useProtectedRoute';
import { useFonts, FiraCode_400Regular, FiraCode_700Bold } from '@expo-google-fonts/fira-code';
import { useEffect } from 'react';
import '../services/authSetup'
import { ModalProvider } from '@/components/modal/ModalContext';
import { Colors } from '@/constants/colors';
import { globalStyles } from '@/constants/globalStyles';

SplashScreen.preventAutoHideAsync();

function RootLayoutNav() {
    useProtectedRoute();
    const { isLoading } = useAuthContext();

    if (isLoading) {
        return (
            <View style={[globalStyles.screen, globalStyles.center]}>
                <ActivityIndicator size="large" color={Colors.mainTheme.primary} />
            </View>
        );
    }

    return (
        <ModalProvider>
            <Stack screenOptions={{ headerShown: false }} />
        </ModalProvider>
    );
}

export default function RootLayout() {
    const [loaded, error] = useFonts({
        'FiraCode-Regular': FiraCode_400Regular,
        'FiraCode-Bold': FiraCode_700Bold,
    });

    useEffect(() => {
        if (loaded || error) {
            SplashScreen.hideAsync();
        }
    }, [loaded, error]);

    if (!loaded && !error) {
        return null;
    }

    return (
        <AuthProvider>
            <RootLayoutNav />
        </AuthProvider>
    );
}
