
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5174,
    headers: {
      'Cross-Origin-Opener-Policy': 'same-origin-allow-popups',
    },
  },
  build: {
    // Rely on Vite default chunking instead of forced single vendor file to fix ESM circular deps
  },
  optimizeDeps: {
    include: ["react-leaflet", "react-leaflet-cluster", "leaflet"],
  },
})
