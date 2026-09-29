import { useEffect } from 'react';
import { useRouter, useSegments } from 'expo-router';
import { useAuth } from './useAuth';

export function useProtectedRoute() {
    const { user, isAuthenticated, isLoading } = useAuth();
    const segments = useSegments();
    const router = useRouter();

    useEffect(() => {
        if (isLoading) return;

        const inAuthGroup = segments[0] === '(auth)';
        const inAdminGroup = segments[0] === '(admin)';

        if (!isAuthenticated && !inAuthGroup) {
            router.replace('/(auth)/login');
            return;
        }

        if (isAuthenticated && inAuthGroup) {
            router.replace('/(main)');
            return;
        }

        if (inAdminGroup && user?.role !== 3) {
            router.replace('/(main)');
            return;
        }
    }, [isAuthenticated, user?.role, isLoading, segments]);
}
