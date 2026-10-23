import { getApiSongsBySongIdCoverImagePresignedUrl } from '@/api';
import { PresignedUrl } from '@/api/types.gen';
import { create } from 'zustand';

interface PresignedUrlsCache {
    imageUrls: Record<string, PresignedUrl>;

    getCoverImageUrl: (songId: string) => Promise<string | null>;
}

export const usePresignedUrlCache = create<PresignedUrlsCache>((set, get) => ({
    imageUrls: {},

    getCoverImageUrl: async (songId: string): Promise<string | null> => {
        const state = get();
        const cached = state.imageUrls[songId];

        if (cached) {
            const expirationTime = new Date(cached.expiresAt).getTime();

            const isExpired = Date.now() >= expirationTime - 10000;

            if (!isExpired) {
                return cached.url;
            }
        }

        try {
            console.log("Fetch Image")
            const result = await getApiSongsBySongIdCoverImagePresignedUrl({
                path: {
                    songId: songId
                }
            });

            if (result.data) {
                console.log(result.data);
                const data = result.data;

                set((state) => {
                    const now = Date.now();
                    const cleanedUrls: Record<string, PresignedUrl> = {};

                    for (const [id, item] of Object.entries(state.imageUrls)) {
                        const expTime = new Date(item.expiresAt).getTime();
                        if (expTime > now) {
                            cleanedUrls[id] = item;
                        }
                    }

                    cleanedUrls[songId] = data;

                    return { imageUrls: cleanedUrls };
                });

                return data.url;
            }
            else {
                console.error(result.error);
                return null;
            }
        } catch (error) {
            console.error("Failed to fetch presigned URL", error);
            return null;
        }
    }
}));