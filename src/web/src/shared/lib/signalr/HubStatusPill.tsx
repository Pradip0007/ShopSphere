import { useNotificationsHub } from './NotificationsHub';

const COLORS: Record<string, string> = {
  idle: '#999',
  connecting: '#eab308',
  connected: '#16a34a',
  reconnecting: '#eab308',
  disconnected: '#dc2626',
};

export function HubStatusPill() {
  const { status } = useNotificationsHub();

  return (
    <span
      title={`hub: ${status}`}
      style={{
        display: 'inline-block',
        width: 10,
        height: 10,
        borderRadius: 999,
        background: COLORS[status] ?? '#999',
      }}
    />
  );
}
