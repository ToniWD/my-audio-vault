/**
 * Formats a duration coming from the API (`number | string`) as `mm:ss`.
 */
export const formatDuration = (duration: number | string | undefined): string => {
    const total = Number(duration ?? 0);

    if (!Number.isFinite(total) || total <= 0) {
        return '--:--';
    }

    const minutes = Math.floor(total / 60);
    const seconds = Math.floor(total % 60);

    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
};