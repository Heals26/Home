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
  // Themes are swapped by `data-theme` on <html>, not by a class, so `dark:` variants are
  // never used — every colour below resolves through a custom property that the theme
  // redefines. Nothing sets `.dark`, so this stays inert.
  darkMode: 'class',
  theme: {
    extend: {
      // Every colour is `rgb(var(--token) / <alpha-value>)` rather than a literal, so one
      // set of utilities serves both themes and opacity modifiers (bg-week/10,
      // border-lights/40) keep working. The values live in wwwroot/css/input.css.
      colors: {
        // The page is "ink" — but read the scale by *role*, not by lightness. 950 is always
        // the page, 900 the surface, 800 raised/borders, 50 the primary text. The light
        // theme inverts the ramp, so those roles hold in both.
        ink: {
          50:  'rgb(var(--ink-50) / <alpha-value>)',
          100: 'rgb(var(--ink-100) / <alpha-value>)',
          200: 'rgb(var(--ink-200) / <alpha-value>)',
          300: 'rgb(var(--ink-300) / <alpha-value>)',
          400: 'rgb(var(--ink-400) / <alpha-value>)',
          500: 'rgb(var(--ink-500) / <alpha-value>)',
          600: 'rgb(var(--ink-600) / <alpha-value>)',
          700: 'rgb(var(--ink-700) / <alpha-value>)',
          800: 'rgb(var(--ink-800) / <alpha-value>)',
          900: 'rgb(var(--ink-900) / <alpha-value>)',
          950: 'rgb(var(--ink-950) / <alpha-value>)',
        },
        // One hue per pillar, used for identity (nav, eyebrows, icons) — not for surfaces.
        // Colour encodes *place* in the app, so a family member can navigate by it.
        recipes:  { DEFAULT: 'rgb(var(--recipes) / <alpha-value>)',   dim: 'rgb(var(--recipes-dim) / <alpha-value>)' },   // apricot
        shopping: { DEFAULT: 'rgb(var(--shopping) / <alpha-value>)',  dim: 'rgb(var(--shopping-dim) / <alpha-value>)' },  // sage
        week:     { DEFAULT: 'rgb(var(--week) / <alpha-value>)',      dim: 'rgb(var(--week-dim) / <alpha-value>)' },      // sky
        lights:   { DEFAULT: 'rgb(var(--lights) / <alpha-value>)',    dim: 'rgb(var(--lights-dim) / <alpha-value>)' },    // lamplight amber
        household:{ DEFAULT: 'rgb(var(--household) / <alpha-value>)', dim: 'rgb(var(--household-dim) / <alpha-value>)' }, // settings stays neutral
        surface: {
          DEFAULT: 'rgb(var(--surface) / <alpha-value>)',
          raised: 'rgb(var(--surface-raised) / <alpha-value>)',
          overlay: 'rgb(var(--surface-overlay) / <alpha-value>)',
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
      // A Surface turned upright is still ~912 CSS pixels wide, so `md:` keeps matching and the
      // landscape layout survives a rotation it was never designed for. `rail:` asks the real
      // question — is this device wide AND lying down? — so upright tablets get the thumb-reachable
      // bottom bar and a single column instead of a left rail and squeezed halves.
      screens: {
        rail: { raw: '(min-width: 768px) and (orientation: landscape)' },
      },
    },
  },
  // v2 needed `variants.extend` to opt into active: styles. v3 enables every variant by
  // default, so the old block is gone — active:scale-95 and active:bg-* just work.
  plugins: [],
}
