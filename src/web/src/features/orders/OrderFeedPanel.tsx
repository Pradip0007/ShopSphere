import { StatusBadge } from './StatusBadge';
import { useAdminOrderFeed } from './useAdminOrderFeed';

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

export function OrderFeedPanel(): React.JSX.Element {
  const events = useAdminOrderFeed();

  return (
    <section className="grid gap-3">
      <div>
        <h2 className="text-lg font-semibold">Live order activity</h2>
        <p className="text-sm text-[var(--color-text-muted)]">Real-time order status updates.</p>
      </div>

      {events.length === 0 ? (
        <p className="rounded-lg border border-[var(--color-border)] p-4 text-sm text-[var(--color-text-muted)]">
          Waiting for order activity…
        </p>
      ) : (
        <ul className="grid gap-2">
          {events.map((event) => (
            <li
              key={`${event.orderId}-${event.timestamp}`}
              className="rounded-lg border border-[var(--color-border)] p-3"
            >
              <div className="flex items-center justify-between gap-4">
                <span className="font-medium">Order {event.orderId.slice(0, 8)}…</span>

                <time dateTime={event.timestamp} className="text-xs text-[var(--color-text-muted)]">
                  {dateTimeFormatter.format(new Date(event.timestamp))}
                </time>
              </div>

              <div className="mt-2">
                <StatusBadge status={event.status} />
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
