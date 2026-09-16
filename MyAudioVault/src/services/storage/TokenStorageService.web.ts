import { ACCESS_TOKEN_KEY, ITokenStorageService, REFRESH_TOKEN_KEY, TokenPair } from "../../types/interfaces/ITokenStorageService";

class TokenStorageService implements ITokenStorageService {

    async storeAccessToken(token: string): Promise<boolean> {
        try {
            localStorage.setItem(ACCESS_TOKEN_KEY, token);
            return true;
        } catch (error) {
            console.error('Error saving access token (Web):', error);
            return false;
        }
    }

    async getAccessToken(): Promise<string | null> {
        try {
            return localStorage.getItem(ACCESS_TOKEN_KEY);
        } catch (error) {
            console.error('Error reading access token (Web):', error);
            return null;
        }
    }

    async removeAccessToken(): Promise<boolean> {
        try {
            localStorage.removeItem(ACCESS_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error deleting access token (Web):', error);
            return false;
        }
    }

    async storeRefreshToken(token: string): Promise<boolean> {
        try {
            localStorage.setItem(REFRESH_TOKEN_KEY, token);
            return true;
        } catch (error) {
            console.error('Error saving refresh token (Web):', error);
            return false;
        }
    }

    async getRefreshToken(): Promise<string | null> {
        try {
            return localStorage.getItem(REFRESH_TOKEN_KEY);
        } catch (error) {
            console.error('Error reading refresh token (Web):', error);
            return null;
        }
    }

    async removeRefreshToken(): Promise<boolean> {
        try {
            localStorage.removeItem(REFRESH_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error deleting refresh token (Web):', error);
            return false;
        }
    }

    async getAccessAndRefreshTokens(): Promise<TokenPair | null> {
        try {
            const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
            const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

            if (accessToken && refreshToken) {
                return {
                    accessToken,
                    refreshToken,
                };
            }

            return null;
        } catch (error) {
            console.error('Error reading the token pair (Web):', error);
            return null;
        }
    }

    async clearAll(): Promise<boolean> {
        try {
            localStorage.removeItem(ACCESS_TOKEN_KEY);
            localStorage.removeItem(REFRESH_TOKEN_KEY);
            return true;
        } catch (error) {
            console.error('Error clearing tokens (Web):', error);
            return false;
        }
    }
}

export const tokenStorage = new TokenStorageService();