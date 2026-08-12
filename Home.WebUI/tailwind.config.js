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
        zinc: {
          50: '#fafafa',
          100: '#f4f4f5',
          200: '#e4e4e7',
          300: '#d4d4d8',
          400: '#a1a1aa',
          500: '#71717a',
          600: '#52525b',
          700: '#3f3f46',
          800: '#27272a',
          900: '#18181b',
          950: '#09090b',
        },
        teal: {
          300: '#5eead4',
          400: '#2dd4bf',
          500: '#14b8a6',
          600: '#0d9488',
        },
        surface: {
          DEFAULT: '#18181b',
          raised: '#27272a',
          overlay: '#09090b',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
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
