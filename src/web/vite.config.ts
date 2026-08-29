import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [react(), tsconfigPaths()],

  server: {
    port: 5173,
    strictPort: true,

    proxy: {
      '/api': {
        target: 'https://localhost:7583',
        changeOrigin: true,
        secure: false,
      },

      '/hubs': {
        target: 'https://localhost:7583',
        changeOrigin: true,
        secure: false,
        ws: true,
      },

      '/graphql': {
        target: 'https://localhost:7583',
        changeOrigin: true,
        secure: false,
      },
    },
  },

  build: {
    target: 'es2022',
    sourcemap: true,
    outDir: 'dist',
  },
});