import { useActionState } from 'react';
import { initialNewsletterState, type NewsletterState, submitNewsletter } from './action';
import { PendingHint } from './PendingHint';
import { SubmitButton } from './SubmitButton';

export function NewsletterForm(): React.JSX.Element {
  const [state, formAction] = useActionState(submitNewsletter, initialNewsletterState);

  return (
    <form
      action={formAction}
      noValidate
      className="grid max-w-md gap-3"
      aria-labelledby="newsletter-heading"
    >
      <h2 id="newsletter-heading" className="text-lg font-semibold">
        Get our newsletter
      </h2>

      <p className="text-sm text-[var(--color-text-muted)]">
        Occasional emails. Unsubscribe anytime.
      </p>

      <div className="grid gap-1">
        <label htmlFor="newsletter-email" className="text-sm font-medium">
          Email address
        </label>

        <input
          id="newsletter-email"
          name="email"
          type="email"
          required
          autoComplete="email"
          aria-invalid={state.fieldErrors['email'] ? 'true' : 'false'}
          aria-describedby={state.fieldErrors['email'] ? 'newsletter-email-error' : undefined}
          className="h-10 rounded-md border border-[var(--color-border)] bg-[var(--color-surface)] px-3"
        />

        {state.fieldErrors['email'] && (
          <span
            id="newsletter-email-error"
            role="alert"
            className="text-sm text-[var(--color-danger)]"
          >
            {state.fieldErrors['email']}
          </span>
        )}
      </div>

      <div className="flex items-center gap-3">
        <SubmitButton />
        <PendingHint />
        <FormMessage state={state} />
      </div>
    </form>
  );
}

function FormMessage({ state }: { state: NewsletterState }): React.JSX.Element | null {
  if (state.status === 'success' && state.message) {
    return (
      <span role="status" className="text-sm text-[var(--color-success)]">
        {state.message}
      </span>
    );
  }

  if (state.status === 'error' && state.message) {
    return (
      <span role="alert" className="text-sm text-[var(--color-danger)]">
        {state.message}
      </span>
    );
  }

  return null;
}
