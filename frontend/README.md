# Petoria Frontend

React 19 + Vite frontend for the [Petoria](../README.md) hotel booking platform.

## Tech Stack

| Technology | Purpose |
|---|---|
| React 19 + Vite | UI framework & build tool |
| React Router DOM v7 | Client-side routing |
| React Context API | Global state (Auth, Language, Currency, Favorites, Cart, Theme) |
| Leaflet + React Leaflet | Interactive hotel map |
| `@react-oauth/google` | Google OAuth login |
| `jwt-decode` | Client-side JWT inspection |
| CSS Modules (plain CSS) | Scoped component styling |
| i18n (custom) | Bilingual EN/BG localisation |

## Getting Started

```bash
npm install
npm run dev
# Opens at http://localhost:5173
```

## Environment

The frontend expects the backend API to be running at `http://localhost:5150` (configured in `src/utils/api.js`).

## Structure

```
src/
├── main.jsx               # App entry point
├── App.jsx                # Root component & routes
├── context/               # React Context providers
│   ├── AuthContext.jsx
│   ├── CartContext.jsx
│   ├── CurrencyContext.jsx
│   ├── FavoritesContext.jsx
│   ├── LanguageContext.jsx
│   └── ThemeContext.jsx
├── components/            # Reusable UI components
├── pages/
│   ├── public/            # Home, About, Deals, Support
│   ├── auth/              # Login, Register
│   ├── hotels/            # Hotel listing, details, management
│   ├── user/              # Cart, Favorites, Reservations, Settings
│   ├── admin/             # Admin dashboard & moderation
│   └── moderator/         # Moderator panel
├── locales/
│   ├── en.json            # English translations
│   └── bg.json            # Bulgarian translations
└── utils/
    └── api.js             # Centralised fetch wrapper
```

## Cross-Browser Compatibility

| Browser | Desktop | Mobile |
|---|---|---|
| Chrome 120+ | ✅ | ✅ |
| Firefox 120+ | ✅ | ✅ |
| Safari 17+ | ✅ | ✅ (iOS) |
| Edge 120+ | ✅ | ✅ |

## Build for Production

```bash
npm run build
# Output in dist/
```
