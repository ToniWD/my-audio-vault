import { getApiSongs, getApiSongsBySongId, getApiSongsBySongIdPresignedUrl, SongDetailsDto } from "@/api";
import { useMusicStore } from "@/hooks/useMusicStore";
import { AudioPlayer, setAudioModeAsync, useAudioPlayer } from "expo-audio";
import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";


export interface AudioPlayerContextType {
    player: AudioPlayer;
    songDetails: null | SongDetailsDto | undefined;
    playlist: SongDetailsDto[];
    autoPlayNextTrack: boolean
    // setSongById: (songId: string | null) => Promise<void>;
    // setSongDetails: (songDetails: SongDetailsDto) => Promise<void>;
    setPlaylist: (songsList: SongDetailsDto[]) => void;
    stop: () => Promise<void>;
    play: () => Promise<void>;
    playFromCurrentPlaylist: (index: number) => Promise<void>;
    next: () => Promise<void>;
    previous: () => Promise<void>;
    generatePlaylist: () => Promise<void>;
    setAutoPlayNextTrack: (enable: boolean) => void;
}

const AudioPlayerContext = createContext<AudioPlayerContextType | undefined>(undefined);

const API_BASE_URL = "http://localhost:5204";

export function AudioPlayerProvider({ children }: { children: React.ReactNode }) {

    // ------------------------------------------------------------ state
    const [autoPlayNextTrack, setAutoPlayNextTrack] = useState<boolean>(true);
    const currentSongId = useMusicStore((state) => state.currentSongId);
    const currentSongIndex = useMusicStore((state) => state.currentSongIndex);
    // setCurrentSongById it will set the index at the first song with the given id
    // from the current playlist
    const setCurrentSongById = useMusicStore((state) => state.setCurrentSongById);
    const setCurrentSongByIndex = useMusicStore((state) => state.setCurrentSongByIndex);

    const playlist = useMusicStore((state) => state.playlist);

    const setPlaylist = useMusicStore((state) => state.setPlaylist);
    const getNextSongIndex = useMusicStore((state) => state.getNextSongIndex);
    const getPrevSongIndex = useMusicStore((state) => state.getPreviousSongIndex);

    const songDetails = useMusicStore((state) => state.currentSongIndex !== null ? state.playlist.at(state.currentSongIndex) : null);

    const player = useAudioPlayer({
        uri: '',
    }, {
        updateInterval: 1000,
        downloadFirst: false,
        preferredForwardBufferDuration: 20
    });

    // ------------------------------------------------------------ effects
    useEffect(() => {
        setAudioModeAsync({
            playsInSilentMode: true,
            shouldPlayInBackground: true,
            interruptionMode: 'doNotMix',
            allowsRecording: false,
            shouldRouteThroughEarpiece: false
        });
    }, []);

    useEffect(() => {
        if (!autoPlayNextTrack) return;
        const subscription = player.addListener("playbackStatusUpdate", async (status) => {
            if (status.didJustFinish) {
                console.log("Song finished, moving to next track...");
                await handleNext();
            }
        });

        return () => {
            subscription.remove();
        };
    }, [player, autoPlayNextTrack]);

    // ------------------------------------------------------------- utils
    const fetchPresignedUrl = async (songId: string): Promise<string | null> => {
        const result = await getApiSongsBySongIdPresignedUrl({
            path: {
                songId: songId
            }
        });

        if (result.data) {
            console.log(result.data);
            return result.data.url as unknown as string;
        }
        else {
            console.error(result.error);
            return null;
        }
    };

    const loadAndPlaySong = async (songId: string) => {
        console.log(`Loading and playing song with ID: ${songId}`);

        var streamUrl = await fetchPresignedUrl(songId);
        if (!streamUrl || streamUrl.length == 0) return;

        player.replace({ uri: streamUrl });
        player.play();
    };

    const fetchSongDetails = useCallback(async (songId: string): Promise<SongDetailsDto | null> => {
        const result = await getApiSongsBySongId({
            path: {
                songId: songId
            }
        });

        if (result.data) {
            return result.data as SongDetailsDto;
        }
        else {
            console.error(result.error);
            return null;
        }
    }, []);

    const handlePlay = useCallback(async () => {
        player.setActiveForLockScreen(true, {
            title: songDetails?.title || undefined,
            artist: undefined,
            albumTitle: undefined,
            artworkUrl: undefined,
        });
        player.play();
    }, [player]);

    const handleStop = useCallback(async () => {
        player.pause();
        player.setActiveForLockScreen(false);
    }, [player]);

    const handleNext = useCallback(async () => {
        const nextSong = getNextSongIndex();
        if (nextSong) {
            setCurrentSongByIndex(nextSong);
            const freshCurrentSongId = useMusicStore.getState().currentSongId;

            if (freshCurrentSongId) {
                await loadAndPlaySong(freshCurrentSongId);
            }
        }
    }, [player]);

    const handlePrevious = useCallback(async () => {
        if (player.currentTime > 3) {
            player.seekTo(0);
            return;
        }

        const prevSong = getPrevSongIndex();

        if (prevSong !== null) {
            setCurrentSongByIndex(prevSong);
            const freshCurrentSongId = useMusicStore.getState().currentSongId;

            if (freshCurrentSongId) {
                await loadAndPlaySong(freshCurrentSongId);
            }
        }
        else {
            player.seekTo(0);
        }
    }, [player]);

    const handleGeneratePlaylist = useCallback(async () => {
        const result = await getApiSongs({
            query: {
                PageNumber: 0,
                PageSize: 20,
                SortBy: undefined,
                Search: undefined
            }
        });

        if (result.error) {
            console.error('Error fetching songs:', result.error);
            return;
        }

        if (result.data?.data && result.data.data.length > 0) {
            setPlaylist(result.data?.data);
            const freshCurrentSongId = useMusicStore.getState().currentSongId;

            if (freshCurrentSongId) {
                await loadAndPlaySong(freshCurrentSongId);
            }
        }
    }, [player]);

    const handleSetPlaylist = useCallback(async (songsList: SongDetailsDto[]) => {
        if (songsList.length === 0) {
            handleStop();
            setPlaylist([]);
            return;
        }

        setPlaylist(songsList);
        const freshCurrentSongId = useMusicStore.getState().currentSongId;

        if (freshCurrentSongId) {
            await loadAndPlaySong(freshCurrentSongId);
        }
    }, [player]);

    const handlePlayFromCurrentPlaylist = useCallback(async (index: number) => {
        const freshPlaylist = useMusicStore.getState().playlist;
        if (index < 0 || index >= freshPlaylist.length) {
            console.error('Index out of bounds for playlist');
            return;
        }

        const freshCurrentSongIndex = useMusicStore.getState().currentSongIndex;
        if (index != freshCurrentSongIndex) {
            setCurrentSongByIndex(index)
            const freshCurrentSongId = useMusicStore.getState().currentSongId;

            if (freshCurrentSongId) {
                await loadAndPlaySong(freshCurrentSongId);
            }
        }
    }, [player]);

    // ------------------------------------------------------------- render
    return (
        <AudioPlayerContext.Provider
            value={{
                player: player,
                playlist: playlist,
                songDetails: songDetails,
                autoPlayNextTrack: autoPlayNextTrack,
                stop: handleStop,
                play: handlePlay,
                next: handleNext,
                previous: handlePrevious,
                generatePlaylist: handleGeneratePlaylist,
                setPlaylist: handleSetPlaylist,
                playFromCurrentPlaylist: handlePlayFromCurrentPlaylist,
                setAutoPlayNextTrack: (enable) => { setAutoPlayNextTrack(enable) },
            }}
        >
            {children}
        </AudioPlayerContext.Provider>
    );
}

export function useAudioPlayerContext(): AudioPlayerContextType {
    const context = useContext(AudioPlayerContext);
    if (!context) {
        throw new Error('useAudioPlayerContext must be used within an AudioPlayerProvider');
    }
    return context;
}