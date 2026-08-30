import { createRootRoute, Link, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';

export const Route = createRootRoute({
  component: RootLayout,
});

function RootLayout(): React.JSX.Element {
  return (
    <>
      <header
        style={{
          display: 'flex',
          gap: '1rem',
          padding: '1rem 2rem',
          borderBottom: '1px solid #ddd',
        }}
      >
        <Link to="/" activeProps={{ style: { fontWeight: 'bold' } }}>
          Home
        </Link>
        <Link to="/products" activeProps={{ style: { fontWeight: 'bold' } }}>
          Products
        </Link>
        <Link to="/login" activeProps={{ style: { fontWeight: 'bold' } }}>
          Login
        </Link>
        <Link to="/register" activeProps={{ style: { fontWeight: 'bold' } }}>
          Register
        </Link>
      </header>
      <main style={{ padding: '2rem' }}>
        <Outlet />
      </main>
      {import.meta.env.DEV && <TanStackRouterDevtools position="bottom-right" />}
    </>
  );
}
