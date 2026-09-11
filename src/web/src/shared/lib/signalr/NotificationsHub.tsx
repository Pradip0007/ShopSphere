import type { HubConnection } from '@microsoft/signalr';
import { createContext, type ReactNode, useContext, useMemo } from 'react';
import { selectAccessToken, selectIsAuthenticated } from '@/store/auth.slice';
import { useAppSelector } from '@/store/hooks';
import { type HubStatus, useSignalR } from './useSignalR';

interface NotificationsHubContextValue {
  connection: HubConnection | null;
  status: HubStatus;
}

const NotificationsHubContext = createContext<NotificationsHubContextValue>({
  connection: null,
  status: 'idle',
});

export function NotificationsHubProvider({ children }: { children: ReactNode }) {
  const accessToken = useAppSelector(selectAccessToken);
  const isAuthenticated = useAppSelector(selectIsAuthenticated);

  const accessTokenFactory = useMemo(() => () => accessToken ?? '', [accessToken]);

  const { connection, status } = useSignalR({
    url: '/hubs/notifications',
    accessTokenFactory,
    enabled: isAuthenticated,
  });

  const value = useMemo(() => ({ connection, status }), [connection, status]);

  return (
    <NotificationsHubContext.Provider value={value}>{children}</NotificationsHubContext.Provider>
  );
}

export function useNotificationsHub(): NotificationsHubContextValue {
  return useContext(NotificationsHubContext);
}
