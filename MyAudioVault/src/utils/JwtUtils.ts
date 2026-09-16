import { AuthUser } from "@/contexts/AuthContext";

export function decodeJwtPayload(token: string): Record<string, unknown> | null {
    try {
        const base64Url = token.split('.')[1];
        if (!base64Url) return null;
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

export function isTokenExpired(token: string): boolean {
    const payload = decodeJwtPayload(token);
    if (!payload || typeof payload.exp !== 'number') return true;
    const now = Math.floor(Date.now() / 1000);
    return payload.exp < now;
}

export function extractUser(token: string): AuthUser | null {
    const payload = decodeJwtPayload(token);
    if (!payload) return null;

    const email = typeof payload.email === 'string' ? payload.email : '';
    const name =
        (typeof payload.name === 'string' && payload.name) ||
        (typeof payload.unique_name === 'string' && payload.unique_name) ||
        email;
    const role = typeof payload.role === 'string' ? payload.role : 'Member';

    return { email, name, role };
}