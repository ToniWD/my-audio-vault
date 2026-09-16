import * as SecureStore from 'expo-secure-store';
import { ACCESS_TOKEN_KEY, ITokenStorageService, REFRESH_TOKEN_KEY, TokenPair } from '../../types/interfaces/ITokenStorageService';

class TokenStorageService implements ITokenStorageService {
    async storeAccessToken(token: string): Promise<boolean> {
        try {
            await SecureStore.setItemAsync(ACCESS_TOKEN_KEY, token);
            return true;
        } catch (error) {
            console.error('Error saving access token (Native):', error);
            return false;
        }
    }

    async getAccessToken(): Promise<string | null> {
        try {
            return await SecureStore.getItemAsync(ACCESS_TOKEN_KEY);
        } catch (error) {
            console.error('Error reading access token (Native):', error);
            return null;
        }
    }

    async removeAccessToken(): Promise<boolean> {
        try {
            await SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error deleting access token (Native):', error);
            return false;
        }
    }

    async storeRefreshToken(token: string): Promise<boolean> {
        try {
            await SecureStore.setItemAsync(REFRESH_TOKEN_KEY, token);
            return true;
        } catch (error) {
            console.error('Error saving refresh token (Native):', error);
            return false;
        }
    }

    async getRefreshToken(): Promise<string | null> {
        try {
            return await SecureStore.getItemAsync(REFRESH_TOKEN_KEY);
        } catch (error) {
            console.error('Error reading refresh token (Native):', error);
            return null;
        }
    }

    async removeRefreshToken(): Promise<boolean> {
        try {
            await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error deleting refresh token (Native):', error);
            return false;
        }
    }

    async getAccessAndRefreshTokens(): Promise<TokenPair | null> {
        try {
            const accessToken = await SecureStore.getItemAsync(ACCESS_TOKEN_KEY);
            const refreshToken = await SecureStore.getItemAsync(REFRESH_TOKEN_KEY);

            if (accessToken && refreshToken) {
                return {
                    accessToken,
                    refreshToken,
                };
            }

            return null;
        } catch (error) {
            console.error('Error reading the token pair (Native):', error);
            return null;
        }
    }

    async clearAll(): Promise<boolean> {
        try {
            await SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY);
            await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error clearing tokens (Native):', error);
            return false;
        }
    }
}

export const tokenStorage = new TokenStorageService();