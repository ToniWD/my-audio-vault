import { getApiUsersMe, LoginRequest, postApiAuthLogin, postApiAuthRegister, RegisterRequest, UserDto } from '@/api';
import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { tokenStorage } from '../services/storage/TokenStorageService'
import { extractUser } from '@/utils/JwtUtils';
import { tokenManager } from '@/services/TokenManager';

export interface AuthContextType {
    user: UserDto | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (data: LoginRequest) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => Promise<void>;
    fetchUser: () => Promise<UserDto | null>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<UserDto | null>(null);
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
                    const result = await fetchUser();
                    console.log(result)
                    if (result) {
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
            await tokenStorage.storeAccessToken(result.data.accessToken);
            await tokenStorage.storeRefreshToken(result.data.refreshToken);
            await fetchUser()
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

    const fetchUser = useCallback(async () => {
        const result = await getApiUsersMe();
        if (result.data) {
            await setUser(result.data);
            return result.data
        } else {
            console.error(result.error);
            return null
        }
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
                fetchUser
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
