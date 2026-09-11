import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { RouterProvider } from '@tanstack/react-router';
import React, { StrictMode } from 'react';
import ReactDOM from 'react-dom';
import { createRoot } from 'react-dom/client';
import { Provider as ReduxProvider } from 'react-redux';
import { bootstrapAuth } from '@/app/boot';
import { router } from '@/router';
import { queryClient } from '@/shared/lib/query-client';
import { NotificationsHubProvider } from '@/shared/lib/signalr/NotificationsHub';
import { store } from '@/store';
import '@/index.css';

if (import.meta.env.DEV) {
  const axeLoader = (await import('@axe-core/react')).default;
  await axeLoader(
    React,
    ReactDOM,
    1000,
    {
      rules: [{ id: 'color-contrast', enabled: true }],
    },
    {
      exclude: [['.TanStackRouterDevtools']],
    },
  );
}

const rootEl = document.getElementById('root');

if (!rootEl) {
  throw new Error('#root not found');
}

await bootstrapAuth();

createRoot(rootEl).render(
  <StrictMode>
    <ReduxProvider store={store}>
      <NotificationsHubProvider>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />

          {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
        </QueryClientProvider>
      </NotificationsHubProvider>
    </ReduxProvider>
  </StrictMode>,
);
