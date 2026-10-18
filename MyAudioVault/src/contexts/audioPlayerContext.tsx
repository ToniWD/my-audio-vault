import { getApiSongsBySongId, postApiSongsTokenBySongId, SongDetailsDto } from "@/api";
import { AudioPlayer, setAudioModeAsync, useAudioPlayer } from "expo-audio";
import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";


export interface AudioPlayerContextType {
    player: AudioPlayer;
    songDetails: null | SongDetailsDto;
    setSongById: (songId: string | null) => Promise<void>;
    setSongDetails: (songDetails: SongDetailsDto) => Promise<void>;
    stop: () => Promise<void>;
    play: () => Promise<void>;
}

const AudioPlayerContext = createContext<AudioPlayerContextType | undefined>(undefined);

const API_BASE_URL = "http://localhost:5204";

export function AudioPlayerProvider({ children }: { children: React.ReactNode }) {
    const [currentSongId, setCurrentSongId] = useState<string | null>(null);

    const currentSongIdRef = useRef<string | null>(null);
    currentSongIdRef.current = currentSongId;

    const [songDetails, setSongDetails] = useState<null | SongDetailsDto>(null);

    const player = useAudioPlayer({
        uri: '',
    }, {
        updateInterval: 1000,
        downloadFirst: false,
        preferredForwardBufferDuration: 20
    });

    useEffect(() => {
        setAudioModeAsync({
            playsInSilentMode: true,
            shouldPlayInBackground: true,
            interruptionMode: 'doNotMix',
            allowsRecording: false,
            shouldRouteThroughEarpiece: false
        });
    }, []);

    const fetchStreamToken = async (songId: string): Promise<string | null> => {
        const result = await postApiSongsTokenBySongId({
            path: {
                songId: songId
            }
        });

        if (result.data) {
            console.log(result.data);
            return result.data as unknown as string;
        }
        else {
            console.error(result.error);
            return null;
        }
    };

    const loadAndPlaySong = async (songId: string) => {
        console.log(`Loading and playing song with ID: ${songId}`);
        const token = await fetchStreamToken(songId);
        if (!token) return;

        const streamUrl = `${API_BASE_URL}/api/songs/stream/${token}`;

        player.replace({ uri: streamUrl });
        player.play();
    };

    useEffect(() => {
        const subscription = player.addListener("playbackStatusUpdate", async (status) => {

            if (status.error) {
                console.warn("Player error, trying to refresh token...", status.error);

                if (currentSongIdRef.current) {
                    await loadAndPlaySong(currentSongIdRef.current);
                }
            }
        });

        return () => {
            subscription.remove();
        };
    }, [player]);

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

    const handleSetSongId = useCallback(async (songId: string | null) => {
        if (!songId || songId.trim() === "") {
            await handleStop();
            setCurrentSongId(null);
            setSongDetails(null);
        } else {
            await fetchSongDetails(songId).then(details => {
                if (details) {
                    setCurrentSongId(details.id);
                    setSongDetails(details);
                }
            });
            await loadAndPlaySong(songId);
        }
    }, []);

    const handleSetSongDetails = useCallback(async (songDetails: SongDetailsDto | null) => {
        if (!songDetails) {
            await handleStop();
            setSongDetails(null);
            setCurrentSongId(null);
        } else {
            setCurrentSongId(songDetails.id);
            setSongDetails(songDetails);
            await loadAndPlaySong(songDetails.id);
        }
    }, []);

    const handlePlay = useCallback(async () => {
        player.setActiveForLockScreen(true, {
            title: songDetails?.title || undefined,
            artist: undefined,
            albumTitle: undefined,
            artworkUrl: songDetails?.coverImageUrl || undefined,
        });
        player.play();
    }, [player]);

    const handleStop = useCallback(async () => {
        player.pause();
        player.setActiveForLockScreen(false);
    }, [player]);

    return (
        <AudioPlayerContext.Provider
            value={{
                player: player,
                songDetails: songDetails,
                setSongById: handleSetSongId,
                setSongDetails: handleSetSongDetails,
                stop: handleStop,
                play: handlePlay,
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