import { createFileRoute, Outlet, redirect } from '@tanstack/react-router';
import { store } from '@/store';
import { selectIsAuthenticated } from '@/store/auth.slice';

export const Route = createFileRoute('/_authed')({
  beforeLoad: ({ location }) => {
    const isAuthed = selectIsAuthenticated(store.getState());

    if (!isAuthed) {
      throw redirect({
        to: '/login',
        search: {
          redirect: location.pathname + location.searchStr,
        },
      });
    }
  },

  component: AuthedLayout,
});

function AuthedLayout(): React.JSX.Element {
  return <Outlet />;
}
