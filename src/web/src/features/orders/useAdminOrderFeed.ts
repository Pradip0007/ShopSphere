import { useEffect, useMemo, useState } from 'react';

import { useNotificationsHub } from '@/shared/lib/signalr/NotificationsHub';
import { useHubGroup } from '@/shared/lib/signalr/useHubGroup';

import type { OrderStatus } from './types';

export interface AdminOrderFeedEvent {
  orderId: string;
  status: OrderStatus;
  timestamp: string;
}

interface SignalROrderStatusEvent {
  orderId: string;
  status: string;
  timestamp: string;
}

const MAX_EVENTS = 50;

const orderStatuses: ReadonlySet<string> = new Set([
  'Pending',
  'Placed',
  'InventoryReserved',
  'PaymentAuthorized',
  'Confirmed',
  'Paid',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Refunded',
]);

function isOrderStatus(status: string): status is OrderStatus {
  return orderStatuses.has(status);
}

export function useAdminOrderFeed(): AdminOrderFeedEvent[] {
  const { connection } = useNotificationsHub();
  const [events, setEvents] = useState<AdminOrderFeedEvent[]>([]);

  const groupArgs = useMemo(() => [], []);

  useHubGroup('JoinAdminOrdersFeed', 'LeaveAdminOrdersFeed', groupArgs);

  useEffect(() => {
    if (!connection) {
      return;
    }

    const handler = (event: SignalROrderStatusEvent) => {
      if (!isOrderStatus(event.status)) {
        return;
      }

      const nextEvent: AdminOrderFeedEvent = {
        orderId: event.orderId,
        status: event.status,
        timestamp: event.timestamp,
      };

      setEvents((current) => [nextEvent, ...current].slice(0, MAX_EVENTS));
    };

    connection.on('orderStatusChanged', handler);

    return () => {
      connection.off('orderStatusChanged', handler);
    };
  }, [connection]);

  return events;
}
