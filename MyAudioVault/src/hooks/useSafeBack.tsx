import { useRouter } from 'expo-router';

export const useSafeBack = () => {
    const router = useRouter();

    const goBackOrFallback = (fallbackRoute: string = '/profile') => {
        if (router.canGoBack()) {
            router.back();
        } else {
            router.replace(fallbackRoute as any);
        }
    };

    return { goBackOrFallback };
};