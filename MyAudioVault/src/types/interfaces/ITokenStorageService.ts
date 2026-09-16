export interface TokenPair {
    accessToken: string;
    refreshToken: string;
}

export const ACCESS_TOKEN_KEY = 'access_token';
export const REFRESH_TOKEN_KEY = 'refresh_token';

export interface ITokenStorageService {
    storeAccessToken(token: string): Promise<boolean>;
    getAccessToken(): Promise<string | null>;
    removeAccessToken(): Promise<boolean>;

    storeRefreshToken(token: string): Promise<boolean>;
    getRefreshToken(): Promise<string | null>;
    removeRefreshToken(): Promise<boolean>;

    getAccessAndRefreshTokens(): Promise<TokenPair | null>;

    clearAll(): Promise<boolean>;
}