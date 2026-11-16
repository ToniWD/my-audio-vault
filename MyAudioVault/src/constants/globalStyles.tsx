import { StyleSheet } from 'react-native';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';

/**
 * Default application stylesheet.
 *
 * Contains the shared building blocks (layout, typography, surfaces, inputs,
 * buttons, overlays) used across screens and components. Anything that is only
 * meaningful for a single screen or component should stay local to that file.
 *
 * Visual language: deep-space HUD, electric cyan accents, sharp corners,
 * uppercase tracking and mono type.
 */
export const globalStyles = StyleSheet.create({
    // ------------------------------------------------------------------ layout
    screen: {
        flex: 1,
        backgroundColor: Colors.mainTheme.background,
        // padding: 20,
    },
    center: {
        alignItems: 'center',
        justifyContent: 'center',
    },
    scrollContent: {
        flexGrow: 1,
        justifyContent: 'center',
        padding: 24,
    },
    formGroup: {
        gap: 8,
    },
    row: {
        flexDirection: 'row',
        gap: 12,
    },
    rowCentered: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
    },
    flex1: {
        flex: 1,
    },
    footer: {
        flexDirection: 'row',
        justifyContent: 'center',
        marginTop: 24,
    },

    // -------------------------------------------------------------- typography
    title: {
        fontSize: 32,
        fontWeight: '700',
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoBold,
        letterSpacing: 2,
        textTransform: 'uppercase',
        marginBottom: 4,
    },
    heading: {
        fontSize: 18,
        fontWeight: '700',
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoBold,
        letterSpacing: 0.6,
    },
    subtitle: {
        fontSize: 14,
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.4,
        marginBottom: 24,
    },
    caption: {
        fontSize: 13,
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.3,
    },
    label: {
        fontSize: 11,
        fontWeight: '600',
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoBold,
        letterSpacing: 1.6,
        textTransform: 'uppercase',
        marginTop: 8,
    },
    eyebrow: {
        color: Colors.mainTheme.textMuted,
        fontSize: 10,
        fontWeight: '700',
        letterSpacing: 3,
        fontFamily: Fonts.monoBold,
        textTransform: 'uppercase',
        marginBottom: 8,
    },
    link: {
        color: Colors.mainTheme.link,
        fontFamily: Fonts.monoBold,
        fontSize: 13,
        fontWeight: '600',
        letterSpacing: 0.8,
        textTransform: 'uppercase',
    },
    pressed: {
        opacity: 0.6,
    },

    // ---------------------------------------------------------------- surfaces
    card: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 4,
        padding: 16,
    },
    cardList: {
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 4,
        overflow: 'hidden',
    },
    listItem: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingVertical: 16,
        paddingHorizontal: 16,
    },
    listItemDivider: {
        borderBottomWidth: 1,
        borderBottomColor: Colors.mainTheme.borderSubtle,
    },
    listItemText: {
        fontSize: 13,
        fontWeight: '500',
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.6,
        textTransform: 'uppercase',
    },
    listItemTextDanger: {
        fontSize: 13,
        fontWeight: '500',
        color: Colors.mainTheme.error,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.6,
        textTransform: 'uppercase',
    },
    avatar: {
        width: 52,
        height: 52,
        borderRadius: 4,
        backgroundColor: Colors.mainTheme.primaryBg,
        borderWidth: 1,
        borderColor: Colors.mainTheme.borderActive,
        alignItems: 'center',
        justifyContent: 'center',
    },
    divider: {
        height: 1,
        backgroundColor: Colors.mainTheme.borderSubtle,
    },

    // ------------------------------------------------------------------ inputs
    input: {
        backgroundColor: Colors.mainTheme.inputBackground,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderLeftWidth: 2,
        borderLeftColor: Colors.mainTheme.primaryMuted,
        borderRadius: 3,
        padding: 14,
        fontSize: 15,
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.4,
    },

    // ----------------------------------------------------------------- buttons
    button: {
        backgroundColor: Colors.mainTheme.surfaceRaised,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 3,
        padding: 16,
        alignItems: 'center',
        marginTop: 16,
    },
    buttonPrimary: {
        backgroundColor: Colors.mainTheme.primaryBg,
        borderColor: Colors.mainTheme.primary,
    },
    buttonDisabled: {
        opacity: 0.4,
    },
    buttonRow: {
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
        width: '100%',
        gap: 12,
    },
    buttonText: {
        color: Colors.mainTheme.primary,
        fontSize: 14,
        fontWeight: '700',
        fontFamily: Fonts.monoBold,
        letterSpacing: 1.6,
        textTransform: 'uppercase',
    },
    buttonTextDanger: {
        color: Colors.mainTheme.error,
        fontSize: 14,
        fontWeight: '700',
        fontFamily: Fonts.monoBold,
        letterSpacing: 1.6,
        textTransform: 'uppercase',
    },
    iconButton: {
        width: 44,
        height: 44,
        borderRadius: 3,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.surfaceRaised,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },
    secondaryButton: {
        width: 48,
        height: 48,
        borderRadius: 3,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.surfaceRaised,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
    },
    playButton: {
        width: 72,
        height: 72,
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.mainTheme.primary,
        shadowColor: Colors.mainTheme.primary,
        shadowOffset: { width: 0, height: 0 },
        shadowOpacity: 0.45,
        shadowRadius: 18,
        elevation: 8,
    },
    toggleButton: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 10,
        backgroundColor: Colors.mainTheme.surfaceRaised,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 3,
        paddingVertical: 12,
        paddingHorizontal: 16,
    },
    toggleText: {
        color: Colors.mainTheme.textPrimary,
        fontSize: 13,
        fontFamily: Fonts.monoBold,
        letterSpacing: 1.2,
        textTransform: 'uppercase',
    },
    toggleBadge: {
        backgroundColor: Colors.mainTheme.primaryBg,
        borderRadius: 2,
        paddingHorizontal: 8,
        paddingVertical: 2,
        borderWidth: 1,
        borderColor: Colors.mainTheme.borderActive,
    },
    toggleBadgeText: {
        color: Colors.mainTheme.primary,
        fontSize: 10,
        letterSpacing: 1,
        fontFamily: Fonts.monoBold,
    },

    // ----------------------------------------------------------------- headers
    headerButtonLeft: {
        marginLeft: 12,
        padding: 4,
    },
    headerButtonRight: {
        marginRight: 12,
        padding: 4,
    },

    // ---------------------------------------------------------------- overlays
    overlay: {
        flex: 1,
        backgroundColor: Colors.mainTheme.scrim,
        alignItems: 'center',
        justifyContent: 'center',
    },
    dialog: {
        width: '80%',
        maxWidth: 500,
        backgroundColor: Colors.mainTheme.surfaceRaised,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderTopWidth: 2,
        borderTopColor: Colors.mainTheme.primary,
        borderRadius: 4,
        padding: 20,
        alignItems: 'center',
        elevation: 8,
        shadowColor: Colors.mainTheme.primary,
        shadowOffset: { width: 0, height: 0 },
        shadowOpacity: 0.18,
        shadowRadius: 18,
    },
    dialogText: {
        fontSize: 14,
        textAlign: 'center',
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoRegular,
        letterSpacing: 0.3,
        marginBottom: 20,
    },
});

const headerStyles = StyleSheet.create({
    headerTitle: {
        fontWeight: '700',
        fontFamily: Fonts.monoBold,
        fontSize: 15,
        letterSpacing: 1.4,
        textTransform: 'uppercase',
        color: Colors.mainTheme.textPrimary,
    },
});

/**
 * Shared `screenOptions` for stacks that render the themed header.
 * Spread into a `Stack` and override only what differs per screen.
 */
export const headerOptions = {
    headerStyle: { backgroundColor: Colors.mainTheme.headerBackground },
    headerTintColor: Colors.mainTheme.primary,
    headerTitleStyle: headerStyles.headerTitle,
    headerShadowVisible: false,
};
