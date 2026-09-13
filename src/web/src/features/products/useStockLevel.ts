import { useEffect, useMemo, useState } from 'react';
import { useNotificationsHub } from '@/shared/lib/signalr/NotificationsHub';
import { useHubGroup } from '@/shared/lib/signalr/useHubGroup';

interface StockChangedEvent {
  sku: string;
  available: number;
  timestamp: string;
}

export function useStockLevel(sku: string, initial: number): number {
  const { connection } = useNotificationsHub();
  const [available, setAvailable] = useState(initial);

  useEffect(() => {
    setAvailable(initial);
  }, [initial]);

  const groupArgs = useMemo(() => [sku], [sku]);
  useHubGroup('JoinStockGroup', 'LeaveStockGroup', groupArgs);

  useEffect(() => {
    if (!connection) return;

    const handler = (evt: StockChangedEvent) => {
      if (evt.sku === sku) {
        setAvailable(evt.available);
      }
    };

    connection.on('stockChanged', handler);

    return () => {
      connection.off('stockChanged', handler);
    };
  }, [connection, sku]);

  return available;
}
