import { refresh } from '@/features/auth/api';
import { getUserFromAccessToken } from '@/shared/lib/jwt';
import { store } from '@/store';
import { logout, setCredentials } from '@/store/auth.slice';

export async function bootstrapAuth(): Promise<void> {
  try {
    const response = await refresh();

    const user = getUserFromAccessToken(response.accessToken);

    store.dispatch(
      setCredentials({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        user,
      }),
    );
  } catch {
    store.dispatch(logout());
  }
}
