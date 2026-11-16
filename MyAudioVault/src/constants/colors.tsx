// Raw palette (HUD / deep-space terminal)
const TerminalPalette = {
    // Deep space blacks & panels
    black: '#000000',
    darkBg: '#05070C',       // Fundal principal, aproape negru cu tentă rece
    surfaceBg: '#0A111C',    // Panouri / carduri / header
    raisedBg: '#0E1725',     // Panouri ridicate (butoane, dialoguri)
    inputBg: '#070D16',      // Fundal pentru input-uri
    borderDark: '#172638',   // Contururi principale
    borderSoft: '#0F1C2C',   // Contururi discrete / separatoare

    // Cyan (electric HUD accent)
    neonCyan: '#22E6FF',     // Accent principal fosforescent
    brightCyan: '#67F3FF',   // Accent aprins (hover / highlight)
    dimCyan: '#22E6FFAA',    // Cyan semi-transparent
    mutedCyan: '#0E7C8C',    // Cyan stins pentru detalii
    deepCyan: '#04222B',     // Fundal pentru elemente active

    // Secondary text & UI
    textPrimary: '#E7F7FF',  // Text principal, alb cu tentă rece
    terminalGray: '#7C94AA', // Text secundar / metadate
    textMuted: '#4C6A82',    // Text discret / eyebrow

    // Alerts & highlights
    neonRed: '#FF3B6B',      // Erori / Alerte
    neonAmber: '#FFB020',    // Avertismente (Warnings)
    neonMint: '#3DFFB0',     // Succes

    // Secondary accent
    electricIndigo: '#7A6CFF',

    // Overlay
    scrim: 'rgba(2, 4, 8, 0.72)', // Fundal pentru overlay-uri (modale)
};

export const Colors = {
    // Tema futuristă / HUD terminal
    mainTheme: {
        // Fundaluri
        background: TerminalPalette.darkBg,
        surface: TerminalPalette.surfaceBg,
        surfaceRaised: TerminalPalette.raisedBg,
        inputBackground: TerminalPalette.inputBg,
        headerBackground: TerminalPalette.black,

        // Text
        textPrimary: TerminalPalette.textPrimary,
        textSecondary: TerminalPalette.terminalGray,
        textMuted: TerminalPalette.textMuted,

        // Elemente Interactive (Butoane, Iconițe)
        primary: TerminalPalette.neonCyan,
        primaryBright: TerminalPalette.brightCyan,
        primaryMuted: TerminalPalette.mutedCyan,
        primaryBg: TerminalPalette.deepCyan,
        headerIcon: TerminalPalette.brightCyan,
        border: TerminalPalette.borderDark,
        borderSubtle: TerminalPalette.borderSoft,
        borderActive: TerminalPalette.neonCyan,

        // Accent secundar
        accent: TerminalPalette.electricIndigo,

        // Stări
        error: TerminalPalette.neonRed,
        warning: TerminalPalette.neonAmber,
        success: TerminalPalette.neonMint,

        // Overlay
        scrim: TerminalPalette.scrim,

        link: TerminalPalette.neonCyan,
        linkHover: TerminalPalette.brightCyan,
    },
};
