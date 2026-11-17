import { getApiSongs, SongDetailsDto } from '@/api';
import { Colors } from '@/constants/colors';
import { globalStyles } from '@/constants/globalStyles';
import { Fonts } from '@/constants/typography';
import Feather from '@react-native-vector-icons/feather';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
    ActivityIndicator,
    Pressable,
    ScrollView,
    StyleSheet,
    Text,
    View,
} from 'react-native';
import SongCard from './songCard';

const DEFAULT_PAGE_SIZE = 24;

const toNumber = (value: number | string | undefined, fallback: number): number => {
    const parsed = Number(value ?? fallback);
    return Number.isFinite(parsed) ? parsed : fallback;
};

export interface SongGridProps {
    pageSize?: number;
    onSongPress?: (song: SongDetailsDto) => void;
}

/**
 * Paginated, fixed-size grid of songs.
 */
export default function SongGrid({ pageSize = DEFAULT_PAGE_SIZE, onSongPress }: SongGridProps) {
    const [page, setPage] = useState(1);
    const [songs, setSongs] = useState<SongDetailsDto[]>([]);
    const [totalPages, setTotalPages] = useState(1);
    const [totalRecords, setTotalRecords] = useState(0);
    const [hasNextPage, setHasNextPage] = useState(false);
    const [hasPreviousPage, setHasPreviousPage] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const requestId = useRef(0);
    const scrollRef = useRef<ScrollView>(null);

    const loadPage = useCallback(async (targetPage: number) => {
        const currentRequest = ++requestId.current;
        setIsLoading(true);
        setError(null);

        const result = await getApiSongs({
            query: {
                PageNumber: targetPage,
                PageSize: pageSize,
                SortBy: undefined,
                Search: undefined,
            },
        });

        // Discard a response for a page the user has already navigated away from.
        if (currentRequest !== requestId.current) return;

        if (result.error) {
            setError('Could not load songs. Please try again.');
            setSongs([]);
            setIsLoading(false);
            return;
        }

        const response = result.data;
        const pages = Math.max(1, toNumber(response?.totalPages, 1));

        setSongs(response?.data ?? []);
        setTotalPages(pages);
        setTotalRecords(toNumber(response?.totalRecords, 0));
        setHasNextPage(response?.hasNextPage ?? targetPage + 1 <= pages);
        setHasPreviousPage(response?.hasPreviousPage ?? targetPage > 1);
        console.log(response.hasPreviousPage)
        setIsLoading(false);
    }, [pageSize]);

    useEffect(() => {
        void loadPage(page);
    }, [page, loadPage]);

    const goToPage = (targetPage: number) => {
        scrollRef.current?.scrollTo({ y: 0, animated: true });
        setPage(targetPage);
    };

    const previousDisabled = !hasPreviousPage || isLoading;
    const nextDisabled = !hasNextPage || isLoading;

    const firstItem = totalRecords === 0 ? 0 : (page - 1) * pageSize + 1;
    const lastItem = Math.min(totalRecords, (page) * pageSize);
    const statusText = totalRecords === 0
        ? 'No songs found'
        : `${firstItem}–${lastItem} of ${totalRecords} songs · Page ${page}/${totalPages}`;

    return (
        <View style={styles.container}>
            {/* ------------------------------------------------------- header */}
            <View style={styles.header}>
                <View style={globalStyles.flex1}>
                    <Text style={globalStyles.eyebrow}>LIBRARY</Text>
                    <Text style={globalStyles.heading}>All songs</Text>
                    <Text style={styles.status}>{statusText}</Text>
                </View>

                <View style={styles.pagination}>
                    <Pressable
                        style={({ pressed }) => [
                            globalStyles.iconButton,
                            pressed && globalStyles.pressed,
                            previousDisabled && globalStyles.buttonDisabled,
                        ]}
                        disabled={previousDisabled}
                        onPress={() => goToPage(page - 1)}
                        accessibilityRole="button"
                        accessibilityLabel="Previous page"
                        accessibilityState={{ disabled: previousDisabled }}
                    >
                        <Feather
                            name="chevron-left"
                            size={20}
                            color={Colors.mainTheme.primary}
                        />
                    </Pressable>

                    <View style={styles.pageBadge}>
                        <Text style={styles.pageBadgeText}>{page}</Text>
                    </View>

                    <Pressable
                        style={({ pressed }) => [
                            globalStyles.iconButton,
                            pressed && globalStyles.pressed,
                            nextDisabled && globalStyles.buttonDisabled,
                        ]}
                        disabled={nextDisabled}
                        onPress={() => goToPage(page + 1)}
                        accessibilityRole="button"
                        accessibilityLabel="Next page"
                        accessibilityState={{ disabled: nextDisabled }}
                    >
                        <Feather
                            name="chevron-right"
                            size={20}
                            color={Colors.mainTheme.primary}
                        />
                    </Pressable>
                </View>
            </View>

            <View style={globalStyles.divider} />

            {/* -------------------------------------------------------- grid */}
            <ScrollView
                ref={scrollRef}
                style={styles.scroll}
                contentContainerStyle={styles.scrollContent}
                showsVerticalScrollIndicator={false}
            >
                {isLoading ? (
                    <View style={styles.state}>
                        <ActivityIndicator size="large" color={Colors.mainTheme.primary} />
                        <Text style={styles.stateText}>Loading songs…</Text>
                    </View>
                ) : error ? (
                    <View style={styles.state}>
                        <Feather
                            name="alert-triangle"
                            size={30}
                            color={Colors.mainTheme.error}
                        />
                        <Text style={styles.stateTitle}>Something went wrong</Text>
                        <Text style={styles.stateText}>{error}</Text>
                        <Pressable
                            style={({ pressed }) => [
                                globalStyles.button,
                                globalStyles.buttonPrimary,
                                styles.retryButton,
                                pressed && globalStyles.pressed,
                            ]}
                            onPress={() => void loadPage(page)}
                            accessibilityRole="button"
                            accessibilityLabel="Retry loading songs"
                        >
                            <Text style={globalStyles.buttonText}>Retry</Text>
                        </Pressable>
                    </View>
                ) : songs.length === 0 ? (
                    <View style={styles.state}>
                        <Feather name="music" size={30} color={Colors.mainTheme.textMuted} />
                        <Text style={styles.stateTitle}>No songs yet</Text>
                        <Text style={styles.stateText}>
                            There is nothing to show on this page.
                        </Text>
                    </View>
                ) : (
                    <View style={styles.grid}>
                        {songs.map((song) => (
                            <SongCard
                                key={song.id}
                                songDetails={song}
                                onPress={onSongPress}
                            />
                        ))}
                    </View>
                )}
            </ScrollView>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        gap: 12,
        paddingHorizontal: 8,
        paddingTop: 12,
    },
    header: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
    },
    status: {
        color: Colors.mainTheme.textMuted,
        fontFamily: Fonts.monoRegular,
        fontSize: 11,
        letterSpacing: 0.6,
        marginTop: 4,
    },
    pagination: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 8,
    },
    pageBadge: {
        minWidth: 44,
        paddingVertical: 10,
        paddingHorizontal: 8,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.primaryBg,
        borderWidth: 1,
        borderColor: Colors.mainTheme.borderActive,
        borderRadius: 2,
    },
    pageBadgeText: {
        color: Colors.mainTheme.primary,
        fontFamily: Fonts.monoBold,
        fontSize: 13,
        fontWeight: '700',
        letterSpacing: 1,
    },
    scroll: {
        flex: 1,
    },
    scrollContent: {
        flexGrow: 1,
        paddingVertical: 12,
    },
    grid: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        justifyContent: 'center',
        gap: 10,
    },
    state: {
        flex: 1,
        minHeight: 220,
        alignItems: 'center',
        justifyContent: 'center',
        gap: 10,
        padding: 24,
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 4,
    },
    stateTitle: {
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoBold,
        fontSize: 14,
        letterSpacing: 0.6,
    },
    stateText: {
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        fontSize: 12,
        letterSpacing: 0.4,
        textAlign: 'center',
    },
    retryButton: {
        alignSelf: 'center',
        marginTop: 4,
        paddingVertical: 10,
        paddingHorizontal: 20,
    },
});
