/** @type {import('tailwindcss').Config} */
module.exports = {
  purge: [
    './**/*.razor',
    './**/*.html',
    './**/*.cshtml',
    '!./node_modules/**/*',
    '!./obj/**/*',
    '!./bin/**/*'
  ],
  darkMode: false,
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
  variants: {
    extend: {
      scale: ['active'],
      backgroundColor: ['active'],
    },
  },
  plugins: [],
}
