import { HubConnectionState } from '@microsoft/signalr';
import { useEffect } from 'react';
import { useNotificationsHub } from './NotificationsHub';

export function useHubGroup(joinMethod: string, leaveMethod: string, args: unknown[]): void {
  const { connection, status } = useNotificationsHub();

  useEffect(() => {
    if (!connection || status !== 'connected') return;

    const hubConnection = connection;
    let joined = false;

    async function join(): Promise<void> {
      if (hubConnection.state !== HubConnectionState.Connected) return;

      try {
        await hubConnection.invoke(joinMethod, ...args);
        joined = true;
      } catch (err) {
        console.error(`${joinMethod} failed`, err);
      }
    }

    void join();

    const rejoin = () => {
      void join();
    };

    hubConnection.onreconnected(rejoin);

    return () => {
      if (joined && hubConnection.state === HubConnectionState.Connected) {
        void hubConnection.invoke(leaveMethod, ...args).catch(() => {});
      }
    };
  }, [connection, status, joinMethod, leaveMethod, args]);
}
