import { type QueryClient, QueryErrorResetBoundary, useQueryClient } from '@tanstack/react-query';
import { createRootRouteWithContext, Link, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import { ErrorBoundary } from 'react-error-boundary';
import { AppError } from '@/app/AppError';
import { logoutApi } from '@/features/auth/api';
import { LogoutButton } from '@/features/auth/LogoutButton';
import { CartBadge } from '@/features/cart/CartBadge';
import { CartDrawer } from '@/features/cart/CartDrawer';
import { NewsletterForm } from '@/features/newsletter/NewsletterForm';
import { router } from '@/router';
import { logToTelemetry } from '@/shared/lib/telemetry';
import { logout, selectAuth, selectHasRole } from '@/store/auth.slice';
import { useAppDispatch, useAppSelector } from '@/store/hooks';

export interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
  notFoundComponent: () => (
    <section className="grid max-w-md mx-auto gap-3 py-12 text-center">
      <h1 className="text-2xl font-semibold">404 — Page not found</h1>
      <p className="text-[var(--color-text-muted)]">The page you're looking for isn't here.</p>
      <a href="/" className="text-brand-600 underline">
        Return home
      </a>
    </section>
  ),
});

function RootLayout(): React.JSX.Element {
  const { user } = useAppSelector(selectAuth);
  const isAdmin = useAppSelector(selectHasRole('admin'));
  const dispatch = useAppDispatch();
  const queryClient = useQueryClient();

  async function handleLogout(): Promise<void> {
    try {
      await logoutApi();
    } finally {
      dispatch(logout());
      queryClient.clear();

      await router.navigate({
        to: '/login',
        search: {},
      });
    }
  }

  return (
    <div className="min-h-screen flex flex-col">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:m-2 focus:rounded-md focus:bg-white focus:px-4 focus:py-2 focus:text-black"
      >
        Skip to main content
      </a>
      <header className="flex items-center gap-4 border-b border-[var(--color-border)] px-8 py-4">
        <nav aria-label="Primary" data-testid="primary-nav" className="flex items-center gap-4">
          <Link to="/" data-testid="nav-home" className="hover:text-brand-600">
            Home
          </Link>

          <Link to="/products" data-testid="nav-products" className="hover:text-brand-600">
            Products
          </Link>

          {user && (
            <CartDrawer
              trigger={
                <button
                  type="button"
                  data-testid="cart-drawer-trigger"
                  aria-label="Open shopping cart"
                  className="rounded px-2 py-1 hover:text-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
                >
                  <CartBadge />
                </button>
              }
            />
          )}

          {user && (
            <Link to="/orders" data-testid="nav-orders" className="hover:text-brand-600">
              Orders
            </Link>
          )}

          {isAdmin && (
            <Link to="/admin" data-testid="nav-admin" className="hover:text-brand-600">
              Admin
            </Link>
          )}
        </nav>

        <div className="ml-auto flex items-center gap-3">
          {user ? (
            <>
              <span data-testid="user-greeting">Hi, {user.email}</span>

              <form action={handleLogout}>
                <LogoutButton />
              </form>
            </>
          ) : (
            <>
              <Link
                to="/login"
                data-testid="nav-login"
                search={{}}
                className="hover:text-brand-600"
              >
                Login
              </Link>

              <Link to="/register" data-testid="nav-register" className="hover:text-brand-600">
                Register
              </Link>
            </>
          )}
        </div>
      </header>
      <main id="main-content" data-testid="main-content" className="flex-1 p-8">
        <QueryErrorResetBoundary>
          {({ reset }) => (
            <ErrorBoundary
              FallbackComponent={AppError}
              onError={(error, info) =>
                logToTelemetry(error, {
                  route: window.location.pathname,
                  extra: { componentStack: info.componentStack },
                })
              }
              onReset={reset}
            >
              <Outlet />
            </ErrorBoundary>
          )}
        </QueryErrorResetBoundary>
      </main>
      <footer
        role="contentinfo"
        className="mt-auto border-t border-[var(--color-border)] px-8 py-8"
      >
        <NewsletterForm />
      </footer>
      {import.meta.env.DEV && (
        <TanStackRouterDevtools position="bottom-right" containerElement="div" />
      )}
    </div>
  );
}
