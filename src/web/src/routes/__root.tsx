import { type QueryClient, useQueryClient } from '@tanstack/react-query';
import { createRootRouteWithContext, Link, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import { logoutApi } from '@/features/auth/api';
import { router } from '@/router';
import { logout, selectAuth, selectHasRole } from '@/store/auth.slice';
import { useAppDispatch, useAppSelector } from '@/store/hooks';

export interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
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
    <>
      <header className="flex items-center gap-4 border-b border-[var(--color-border)] px-8 py-4">
        <Link to="/" className="hover:text-brand-600">
          Home
        </Link>

        <Link to="/products" className="hover:text-brand-600">
          Products
        </Link>

        {user && (
          <Link to="/cart" className="hover:text-brand-600">
            Cart
          </Link>
        )}

        {user && (
          <Link to="/orders" className="hover:text-brand-600">
            Orders
          </Link>
        )}

        {isAdmin && (
          <Link to="/admin" className="hover:text-brand-600">
            Admin
          </Link>
        )}

        <div className="ml-auto flex items-center gap-3">
          {user ? (
            <>
              <span>Hi, {user.email}</span>

              <button
                type="button"
                onClick={handleLogout}
                className="rounded-md border border-[var(--color-border)] px-3 py-1.5 text-sm hover:bg-[var(--color-surface-muted)]"
              >
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" search={{}} className="hover:text-brand-600">
                Login
              </Link>

              <Link to="/register" className="hover:text-brand-600">
                Register
              </Link>
            </>
          )}
        </div>
      </header>

      <main className="p-8">
        <Outlet />
      </main>

      {import.meta.env.DEV && <TanStackRouterDevtools position="bottom-right" />}
    </>
  );
}
