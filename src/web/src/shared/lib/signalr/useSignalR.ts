import {
  type HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';

export type HubStatus = 'idle' | 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

export interface UseSignalROptions {
  /** Full hub URL, e.g. /hubs/notifications */
  url: string;

  /** Called by SignalR whenever it needs the current JWT. */
  accessTokenFactory: () => string | Promise<string>;

  /** Set false to defer connect when the user is not authenticated. */
  enabled?: boolean;
}

export interface UseSignalRResult {
  connection: HubConnection | null;
  status: HubStatus;
}

export function useSignalR({
  url,
  accessTokenFactory,
  enabled = true,
}: UseSignalROptions): UseSignalRResult {
  const connectionRef = useRef<HubConnection | null>(null);
  const [status, setStatus] = useState<HubStatus>('idle');

  useEffect(() => {
    if (!enabled) {
      setStatus('idle');
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory })
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(LogLevel.Information)
      .build();

    connectionRef.current = connection;

    connection.onreconnecting(() => {
      setStatus('reconnecting');
    });

    connection.onreconnected(() => {
      setStatus('connected');
    });

    connection.onclose(() => {
      setStatus('disconnected');
    });

    setStatus('connecting');

    connection
      .start()
      .then(() => {
        setStatus('connected');
      })
      .catch((err) => {
        console.error('SignalR start failed', err);
        setStatus('disconnected');
      });

    return () => {
      if (connection.state !== HubConnectionState.Disconnected) {
        connection.stop().catch(() => {});
      }

      connectionRef.current = null;
    };
  }, [url, enabled, accessTokenFactory]);

  return {
    connection: connectionRef.current,
    status,
  };
}
