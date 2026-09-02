import { createFileRoute, Outlet, redirect } from '@tanstack/react-router';
import { store } from '@/store';
import { selectAuth, selectIsAuthenticated } from '@/store/auth.slice';

const ADMIN_ROLE = 'admin';

export const Route = createFileRoute('/admin')({
  beforeLoad: ({ location }) => {
    const state = store.getState();

    if (!selectIsAuthenticated(state)) {
      throw redirect({
        to: '/login',
        search: {
          redirect: location.pathname + location.searchStr,
        },
      });
    }

    const roles = selectAuth(state).user?.roles ?? [];

    if (!roles.includes(ADMIN_ROLE)) {
      throw redirect({
        to: '/forbidden',
      });
    }
  },

  component: AdminLayout,
});

function AdminLayout(): React.JSX.Element {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '220px 1fr',
        gap: '1.5rem',
      }}
    >
      <aside
        style={{
          borderRight: '1px solid #ddd',
          paddingRight: '1rem',
        }}
      >
        <h3 style={{ marginTop: 0 }}>Admin</h3>

        <nav
          style={{
            display: 'grid',
            gap: 8,
          }}
        >
          <a href="/admin/products">Products</a>
          <a href="/admin/orders">Orders</a>
          <a href="/admin/users">Users</a>
        </nav>
      </aside>

      <section>
        <Outlet />
      </section>
    </div>
  );
}
