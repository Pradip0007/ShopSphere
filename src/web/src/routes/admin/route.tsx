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
    <div className="grid grid-cols-[220px_1fr] gap-6">
      <aside className="border-r border-[var(--color-border)] pr-4">
        <h3 className="mt-0">Admin</h3>

        <nav className="grid gap-2">
          <a href="/admin/products" className="hover:text-brand-600">
            Products
          </a>

          <a href="/admin/orders" className="hover:text-brand-600">
            Orders
          </a>

          <a href="/admin/users" className="hover:text-brand-600">
            Users
          </a>
        </nav>
      </aside>

      <section>
        <Outlet />
      </section>
    </div>
  );
}
