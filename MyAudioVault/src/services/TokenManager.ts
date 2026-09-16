import { isTokenExpired } from "@/utils/JwtUtils";
import { tokenStorage } from "./storage/TokenStorageService.web";
import { postApiAuthRefresh } from "@/api";
import { publicClient } from "./authSetup";

class TokenManager {
    private _refreshPromise: Promise<string | null> | null = null;
    private _onSessionExpired: (() => void) | null = null;

    setOnSessionExpired(cb: () => void) { this._onSessionExpired = cb; }
    clearSessionExpiredCallback() { this._onSessionExpired = null; }

    async getValidAccessToken(): Promise<string | null> {
        const token = await tokenStorage.getAccessToken();
        if (token && !isTokenExpired(token)) return token;

        return this._doRefresh();
    }

    private _doRefresh(): Promise<string | null> {
        if (!this._refreshPromise) {
            this._refreshPromise = this._refresh().finally(() => {
                this._refreshPromise = null;
            });
        }
        return this._refreshPromise;
    }

    private async _refresh(): Promise<string | null> {
        console.log("Attempt to refresh the access token")
        const refreshToken = await tokenStorage.getRefreshToken();

        if (!refreshToken || isTokenExpired(refreshToken)) {
            this._onSessionExpired?.();
            return null;
        }

        try {
            const result = await postApiAuthRefresh({
                body: { refreshToken },
                throwOnError: true,
                client: publicClient
            });

            if (result.data) {
                await tokenStorage.storeAccessToken(result.data.accessToken);
                await tokenStorage.storeRefreshToken(result.data.refreshToken);
                console.log("Access token successfully refreshed.")

                return result.data.accessToken;
            }

            this._onSessionExpired?.();
            return null;
        } catch {
            this._onSessionExpired?.();
            return null;
        }
    }

    async logout(): Promise<void> {
        this._refreshPromise = null;
        await tokenStorage.clearAll()
        console.log("Logout");
    }
}

export const tokenManager = new TokenManager();