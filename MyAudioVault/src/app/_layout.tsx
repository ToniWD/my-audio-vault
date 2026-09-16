import { Stack } from 'expo-router';
import { ActivityIndicator, View } from 'react-native';
import { AuthProvider, useAuthContext } from '../contexts/AuthContext';
import { useProtectedRoute } from '../hooks/useProtectedRoute';
import '../services/authSetup'

function RootLayoutNav() {
    useProtectedRoute();
    const { isLoading } = useAuthContext();

    if (isLoading) {
        return (
            <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: '#0f0f0f' }}>
                <ActivityIndicator size="large" color="#6366f1" />
            </View>
        );
    }

    return <Stack screenOptions={{ headerShown: false }} />;
}

export default function RootLayout() {
    return (
        <AuthProvider>
            <RootLayoutNav />
        </AuthProvider>
    );
}
