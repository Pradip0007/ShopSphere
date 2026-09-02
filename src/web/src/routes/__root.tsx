import type { QueryClient } from '@tanstack/react-query';
import { createRootRouteWithContext, Link, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import { logoutApi } from '@/features/auth/api';
import { router } from '@/router';
import { logout, selectAuth } from '@/store/auth.slice';
import { useAppDispatch, useAppSelector } from '@/store/hooks';

export interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
});

function RootLayout(): React.JSX.Element {
  const { user } = useAppSelector(selectAuth);
  const dispatch = useAppDispatch();

  async function handleLogout(): Promise<void> {
    try {
      await logoutApi();
    } finally {
      dispatch(logout());

      await router.navigate({
        to: '/login',
        search: {},
      });
    }
  }

  return (
    <>
      <header
        style={{
          display: 'flex',
          gap: '1rem',
          alignItems: 'center',
          padding: '1rem 2rem',
          borderBottom: '1px solid #ddd',
        }}
      >
        <Link to="/">Home</Link>

        <Link to="/products">Products</Link>

        <div
          style={{
            marginLeft: 'auto',
            display: 'flex',
            gap: '0.75rem',
          }}
        >
          {user ? (
            <>
              <span>Hi, {user.email}</span>

              <button type="button" onClick={handleLogout}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" search={{}}>
                Login
              </Link>

              <Link to="/register">Register</Link>
            </>
          )}
        </div>
      </header>

      <main style={{ padding: '2rem' }}>
        <Outlet />
      </main>

      {import.meta.env.DEV && <TanStackRouterDevtools position="bottom-right" />}
    </>
  );
}
