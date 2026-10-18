import { SongDetailsDto } from '@/api/types.gen';
import { create } from 'zustand';

interface MusicState {
    playlist: SongDetailsDto[];
    currentSongId: string | null;
    isLoading: boolean;

    setPlaylist: (songsList: SongDetailsDto[]) => void;
    addToPlaylist: (song: SongDetailsDto) => void;
    removeFromPlaylist: (songId: number | string) => void;
    setIsLoading: (loading: boolean) => void;
    getNextSong: () => SongDetailsDto | null;
    getPreviousSong: () => SongDetailsDto | null;
    setCurrentSongId: (songId: string | null) => void;
}

export const useMusicStore = create<MusicState>((set, get) => ({
    playlist: [],
    currentSongId: null,
    isLoading: false,

    setPlaylist: (songsList: SongDetailsDto[]) => set({ playlist: songsList }),

    setIsLoading: (loading: boolean) => set({ isLoading: loading }),

    addToPlaylist: (song: SongDetailsDto) => {
        const { playlist } = get();
        const exists = playlist.some((item) => item.id === song.id);

        if (!exists) {
            set({ playlist: [...playlist, song] });
        }
    },

    removeFromPlaylist: (songId: number | string) => {
        set((state) => ({
            playlist: state.playlist.filter((song) => song.id !== songId),
        }));
    },

    getNextSong: () => {
        const { playlist, currentSongId } = get();
        if (!playlist || playlist.length === 0) return null;
        if (!currentSongId) return playlist[0];

        const currentIndex = playlist.findIndex((song) => song.id === currentSongId);
        if (currentIndex === -1 || currentIndex + 1 >= playlist.length) return null;

        return playlist[currentIndex + 1];
    },

    getPreviousSong: () => {
        const { playlist, currentSongId } = get();
        if (!playlist || playlist.length === 0) return null;
        if (!currentSongId) return playlist[0];

        const currentIndex = playlist.findIndex((song) => song.id === currentSongId);
        if (currentIndex <= 0) return null;

        return playlist[currentIndex - 1];
    },

    setCurrentSongId: (songId: string | null) => set({ currentSongId: songId }),

}));