// Culori brute (Palette)
const TerminalPalette = {
    // Pure Black & Dark Charcoal
    black: '#000000',
    darkBg: '#0D1117',       // Fundal închis cu tentă subtilă
    surfaceBg: '#161B22',    // Fundal pentru carduri / input-uri / header
    borderDark: '#21262D',   // Contururi discrete

    // Greens (Neon & Muted)
    neonGreen: '#00FF66',    // Verde clasic fosforescent (Matrix / Monocrom)
    brightGreen: '#39FF14',  // Accent verde aprins
    dimGreen: '#00FF66AA',   // Verde semi-transparent pentru opacitate
    mutedGreen: '#009944',  // Verde mai stins pentru detalii
    darkGreen: '#052E16',    // Fundal pentru butoane sau badge-uri active

    // Secondary Text & UI
    terminalGray: '#8B949E', // Text secundar / comentarii de cod

    // Alerts & Highlights
    neonRed: '#FF3366',      // Erori / Alerte
    neonAmber: '#FFB000',    // Avertismente (Warnings)

    // Links
    neonCyan: '#00F0FF',

    // Overlay
    scrim: 'rgba(0, 0, 0, 0.5)',       // Fundal pentru overlay-uri (modale)
};

export const Colors = {
    // Tema Terminal / Hacker Mode
    mainTheme: {
        // Fundaluri
        background: TerminalPalette.darkBg,
        surface: TerminalPalette.surfaceBg,
        headerBackground: TerminalPalette.black,

        // Text
        textPrimary: TerminalPalette.neonGreen,    // Textul principal verde aprins
        textSecondary: TerminalPalette.terminalGray, // Subtitluri, placeholder-i, date
        textMuted: TerminalPalette.mutedGreen,     // Text discret/secundar verde

        // Elemente Interactive (Butoane, Iconițe)
        primary: TerminalPalette.neonGreen,
        primaryBg: TerminalPalette.darkGreen,
        headerIcon: TerminalPalette.neonGreen,
        border: TerminalPalette.borderDark,
        borderActive: TerminalPalette.neonGreen,

        // Stări
        error: TerminalPalette.neonRed,
        warning: TerminalPalette.neonAmber,
        success: TerminalPalette.brightGreen,

        // Overlay
        scrim: TerminalPalette.scrim,

        link: TerminalPalette.neonCyan,          // sau TerminalPalette.electricIndigo
        linkHover: TerminalPalette.brightGreen,
    },

    // Poți păstra și alte teme aici dacă dorești (ex: light, dark)
};