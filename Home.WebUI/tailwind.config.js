/** @type {import('tailwindcss').Config} */
module.exports = {
  // v3 renamed `purge` to `content`, and scanning is always on — there is no longer a
  // NODE_ENV gate, so the output is only ever the classes this project actually uses.
  content: [
    './**/*.razor',
    './**/*.html',
    './**/*.cshtml',
    '!./node_modules/**/*',
    '!./obj/**/*',
    '!./bin/**/*'
  ],
  // The app is dark-only by design — the palette below IS the dark palette and there is no
  // toggle. 'class' means `dark:` variants never activate, since nothing sets the class.
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Warm near-black neutrals (Tailwind stone) — the page is "ink", surfaces are "char".
        // Kept under the `ink` name so the intent is readable in markup.
        ink: {
          50:  '#fafaf9',
          100: '#f5f5f4',
          200: '#e7e5e4',
          300: '#d6d3d1',
          400: '#a8a29e',
          500: '#78716c',
          600: '#57534e',
          700: '#44403c',
          800: '#292524',
          900: '#1c1917',
          950: '#0c0a09',
        },
        // One hue per pillar, used for identity (nav, eyebrows, icons) — not for surfaces.
        // Colour encodes *place* in the app, so a family member can navigate by it.
        recipes:  { DEFAULT: '#fb923c', dim: '#c2410c' },   // apricot
        shopping: { DEFAULT: '#a3b18a', dim: '#588157' },   // sage
        week:     { DEFAULT: '#7dd3fc', dim: '#0369a1' },   // sky
        lights:   { DEFAULT: '#fbbf24', dim: '#b45309' },   // lamplight amber
        household:{ DEFAULT: '#d6d3d1', dim: '#78716c' },   // settings stays neutral
        surface: {
          DEFAULT: '#1c1917',
          raised: '#292524',
          overlay: '#0c0a09',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        display: ['Fraunces', 'Georgia', 'serif'],
      },
      minHeight: {
        '12': '3rem',
        '14': '3.5rem',
      },
    },
  },
  // v2 needed `variants.extend` to opt into active: styles. v3 enables every variant by
  // default, so the old block is gone — active:scale-95 and active:bg-* just work.
  plugins: [],
}
