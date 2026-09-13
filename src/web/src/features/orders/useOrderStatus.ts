import { useEffect, useMemo, useState } from 'react';

import { useNotificationsHub } from '@/shared/lib/signalr/NotificationsHub';
import { useHubGroup } from '@/shared/lib/signalr/useHubGroup';

interface OrderStatusChangedEvent {
  orderId: string;
  status: string;
  timestamp: string;
}

export function useOrderStatus(orderId: string, initial: string): string {
  const { connection } = useNotificationsHub();
  const [status, setStatus] = useState(initial);

  useEffect(() => {
    setStatus(initial);
  }, [initial]);

  const groupArgs = useMemo(() => [orderId], [orderId]);

  useHubGroup('JoinOrderGroup', 'LeaveOrderGroup', groupArgs);

  useEffect(() => {
    if (!connection) {
      return;
    }

    const handler = (evt: OrderStatusChangedEvent) => {
      if (evt.orderId === orderId) {
        setStatus(evt.status);
      }
    };

    connection.on('orderStatusChanged', handler);

    return () => {
      connection.off('orderStatusChanged', handler);
    };
  }, [connection, orderId]);

  return status;
}
