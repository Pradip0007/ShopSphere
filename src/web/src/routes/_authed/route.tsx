import { createFileRoute, Outlet, redirect } from '@tanstack/react-router';
import { store } from '@/store';

export const Route = createFileRoute('/_authed')({
  beforeLoad: ({ location }) => {
    const accessToken = store.getState().auth.accessToken;

    if (!accessToken) {
      throw redirect({
        to: '/login',
        search: {
          redirect: location.pathname,
        },
      });
    }
  },

  component: AuthedLayout,
});

function AuthedLayout(): React.JSX.Element {
  return (
    <section>
      <p style={{ fontSize: '0.85rem', opacity: 0.6 }}>[authed area]</p>

      <Outlet />
    </section>
  );
}
