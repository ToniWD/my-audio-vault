import { StyleSheet } from 'react-native';
import { Colors } from '@/constants/colors';
import { Fonts } from '@/constants/typography';

/**
 * Default application stylesheet.
 *
 * Contains the shared building blocks (layout, typography, surfaces, inputs,
 * buttons, overlays) used across screens and components. Anything that is only
 * meaningful for a single screen or component should stay local to that file.
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
        fontFamily: Fonts.monoRegular,
        marginBottom: 4,
    },
    heading: {
        fontSize: 18,
        fontWeight: '700',
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoRegular,
    },
    subtitle: {
        fontSize: 16,
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        marginBottom: 24,
    },
    caption: {
        fontSize: 14,
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
    },
    label: {
        fontSize: 14,
        fontWeight: '500',
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
        marginTop: 8,
    },
    link: {
        color: Colors.mainTheme.link,
        fontFamily: Fonts.monoRegular,
        fontSize: 14,
        fontWeight: '600',
    },

    // ---------------------------------------------------------------- surfaces
    card: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
        padding: 16,
    },
    cardList: {
        backgroundColor: Colors.mainTheme.surface,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 16,
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
        borderBottomColor: Colors.mainTheme.border,
    },
    listItemText: {
        fontSize: 14,
        fontWeight: '500',
        color: Colors.mainTheme.textSecondary,
        fontFamily: Fonts.monoRegular,
    },
    listItemTextDanger: {
        fontSize: 14,
        fontWeight: '500',
        color: Colors.mainTheme.error,
        fontFamily: Fonts.monoRegular,
    },
    avatar: {
        width: 52,
        height: 52,
        borderRadius: 26,
        backgroundColor: Colors.mainTheme.background,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        alignItems: 'center',
        justifyContent: 'center',
    },

    // ------------------------------------------------------------------ inputs
    input: {
        backgroundColor: Colors.mainTheme.background,
        borderWidth: 1,
        borderColor: Colors.mainTheme.border,
        borderRadius: 12,
        padding: 14,
        fontSize: 16,
        color: Colors.mainTheme.textMuted,
        fontFamily: Fonts.monoRegular,
    },

    // ----------------------------------------------------------------- buttons
    button: {
        backgroundColor: Colors.mainTheme.surface,
        borderColor: Colors.mainTheme.border,
        borderRadius: 12,
        padding: 16,
        alignItems: 'center',
        marginTop: 16,
    },
    buttonPrimary: {
        backgroundColor: Colors.mainTheme.primaryBg,
    },
    buttonDisabled: {
        opacity: 0.6,
    },
    buttonRow: {
        flexDirection: 'row',
        justifyContent: 'center',
        alignItems: 'center',
        width: '100%',
        gap: 12,
    },
    buttonText: {
        color: Colors.mainTheme.textPrimary,
        fontSize: 16,
        fontWeight: '600',
        fontFamily: Fonts.monoBold,
    },
    buttonTextDanger: {
        color: Colors.mainTheme.error,
        fontSize: 16,
        fontWeight: '600',
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
        backgroundColor: Colors.mainTheme.background,
        borderRadius: 14,
        padding: 20,
        alignItems: 'center',
        elevation: 5,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.25,
        shadowRadius: 4,
    },
    dialogText: {
        fontSize: 15,
        textAlign: 'center',
        color: Colors.mainTheme.textPrimary,
        fontFamily: Fonts.monoRegular,
        marginBottom: 20,
    },
});

const headerStyles = StyleSheet.create({
    headerTitle: {
        fontWeight: '600',
        fontFamily: Fonts.monoRegular,
        color: Colors.mainTheme.textPrimary,
    },
});

/**
 * Shared `screenOptions` for stacks that render the themed header.
 * Spread into a `Stack` and override only what differs per screen.
 */
export const headerOptions = {
    headerStyle: { backgroundColor: Colors.mainTheme.surface },
    headerTintColor: Colors.mainTheme.border,
    headerTitleStyle: headerStyles.headerTitle,
    headerShadowVisible: false,
};
