import { LoginRequest, postApiAuthLogin, postApiAuthRegister, RegisterRequest } from '@/api';
import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { tokenStorage } from '../services/storage/TokenStorageService'
import { extractUser } from '@/utils/JwtUtils';
import { tokenManager } from '@/services/TokenManager';

export interface AuthUser {
    email: string;
    name: string;
    role: string;
}

export interface AuthContextType {
    user: AuthUser | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (data: LoginRequest) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<AuthUser | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        tokenManager.setOnSessionExpired(() => {
            console.log("Session expired.")
            setUser(null);
            tokenStorage.clearAll();
        });

        const initAuth = async () => {
            try {
                const token = await tokenManager.getValidAccessToken();

                if (token) {
                    const authUser = extractUser(token);
                    if (authUser) {
                        setUser(authUser);
                        return;
                    }
                }

                await logout();
            } catch (error) {
                console.error('Auth init error:', error);
                await logout();
            } finally {
                setIsLoading(false);
            }
        };

        initAuth();

        return () => {
            tokenManager.clearSessionExpiredCallback();
        };
    }, []);

    const login = useCallback(async (data: LoginRequest) => {
        const result = await postApiAuthLogin({
            body: data
        });
        if (result.data) {
            const authUser = extractUser(result.data.accessToken);
            await tokenStorage.storeAccessToken(result.data.accessToken);
            await tokenStorage.storeRefreshToken(result.data.refreshToken);
            setUser(authUser);
        } else {
            console.error(result.error);
        }
    }, []);

    const register = useCallback(async (data: RegisterRequest) => {
        const result = await postApiAuthRegister({
            throwOnError: true,
            body: data
        });
        if (data.email && data.password)
            await login({ email: data.email, password: data.password });
    }, []);

    const logout = useCallback(async () => {
        setUser(null);
        tokenManager.logout();
    }, []);

    return (
        <AuthContext.Provider
            value={{
                user,
                isAuthenticated: user !== null,
                isLoading,
                login,
                register,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuthContext(): AuthContextType {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}
