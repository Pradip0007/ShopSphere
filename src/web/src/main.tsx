import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { RouterProvider } from '@tanstack/react-router';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Provider as ReduxProvider } from 'react-redux';
import { bootstrapAuth } from '@/app/boot';
import { router } from '@/router';
import { queryClient } from '@/shared/lib/query-client';
import { store } from '@/store';
import '@/index.css';

const rootEl = document.getElementById('root');

if (!rootEl) {
  throw new Error('#root not found');
}

await bootstrapAuth();

createRoot(rootEl).render(
  <StrictMode>
    <ReduxProvider store={store}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />

        {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
      </QueryClientProvider>
    </ReduxProvider>
  </StrictMode>,
);
