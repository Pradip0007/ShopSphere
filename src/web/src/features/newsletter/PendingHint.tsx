import { useFormStatus } from 'react-dom';

export function PendingHint(): React.JSX.Element | null {
  const { pending } = useFormStatus();

  if (!pending) return null;

  return (
    <span className="text-xs text-[var(--color-text-muted)]" aria-live="polite">
      Contacting server…
    </span>
  );
}
