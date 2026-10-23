import { SongDetailsDto } from '@/api/types.gen';
import { create } from 'zustand';

interface MusicState {
    playlist: SongDetailsDto[];
    currentSongId: string | null;
    currentSongIndex: number | null;
    isLoading: boolean;

    setPlaylist: (songsList: SongDetailsDto[]) => void;
    removeFromPlaylist: (songIndex: number) => void;
    setIsLoading: (loading: boolean) => void;
    getNextSongIndex: () => number | null;
    getPreviousSongIndex: () => number | null;
    setCurrentSongById: (songId: string | null) => void;
    setCurrentSongByIndex: (index: number | null) => void;
}

export const useMusicStore = create<MusicState>((set, get) => ({
    playlist: [],
    currentSongId: null,
    currentSongIndex: null,
    isLoading: false,

    setPlaylist: (songsList: SongDetailsDto[],) => set({
        playlist: songsList,
        currentSongIndex: songsList.length > 0 ? 0 : null,
        currentSongId: songsList.length > 0 ? songsList[0].id : null
    }),

    setIsLoading: (loading: boolean) => set({ isLoading: loading }),

    removeFromPlaylist: (songIndex: number) => {
        const { playlist } = get();
        if (songIndex < 0 || songIndex >= playlist.length) {
            console.error('Index out of bounds for playlist');
            return;
        }

        const newPlaylist = [...playlist];
        newPlaylist.splice(songIndex, 1);
        set({ playlist: newPlaylist });
    },

    getNextSongIndex: () => {
        const { playlist, currentSongIndex } = get();
        if (!playlist || playlist.length === 0) return null;
        if (currentSongIndex === null) return 0;

        if (currentSongIndex + 1 >= playlist.length) return null;

        return currentSongIndex + 1;
    },

    getPreviousSongIndex: () => {
        const { playlist, currentSongIndex } = get();
        if (!playlist || playlist.length === 0) return null;
        if (currentSongIndex === null) return 0;

        if (currentSongIndex <= 0) return null;

        return currentSongIndex - 1;
    },

    setCurrentSongById: (songId: string | null) => {
        const { playlist } = get();
        const index = playlist.findIndex((song) => song.id === songId);
        if (index !== -1) {
            set({ currentSongId: songId, currentSongIndex: index });
        } else {
            console.error('Song ID not found in playlist');
        }
    },
    setCurrentSongByIndex: (index: number | null) => {
        const { playlist } = get();
        if (index !== null && (index < 0 || index >= playlist.length)) {
            console.error('Index out of bounds for playlist');
            return;
        }

        const songId = index !== null ? playlist[index].id : null;
        set({ currentSongId: songId, currentSongIndex: index });
    },
}));