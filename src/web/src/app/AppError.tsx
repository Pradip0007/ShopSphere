import type { FallbackProps } from 'react-error-boundary';
import { ApiError } from '@/shared/lib/api-error';
import { Button } from '@/shared/ui';

export function AppError({ error, resetErrorBoundary }: FallbackProps): React.JSX.Element {
  const isApi = error instanceof ApiError;
  const status = isApi ? error.status : undefined;
  const title = isApi ? apiTitle(status) : 'Something went wrong';

  const message =
    isApi && error.problem?.detail
      ? error.problem.detail
      : error instanceof Error
        ? error.message
        : 'An unexpected error occurred.';

  return (
    <section role="alert" className="grid max-w-lg mx-auto gap-4 py-12">
      <h1 className="text-2xl font-semibold">{title}</h1>

      <p className="text-[var(--color-text-muted)]">{message}</p>

      {import.meta.env.DEV && error instanceof Error && error.stack && (
        <pre className="overflow-auto rounded bg-[var(--color-surface-muted)] p-3 text-xs">
          {error.stack}
        </pre>
      )}

      <div className="flex gap-2">
        <Button onClick={resetErrorBoundary}>Try again</Button>

        <Button asChild variant="ghost">
          <a href="/">Return home</a>
        </Button>
      </div>
    </section>
  );
}

function apiTitle(status: number | undefined): string {
  switch (status) {
    case 400:
    case 422:
      return 'Invalid request';
    case 401:
      return 'Please sign in';
    case 403:
      return 'Forbidden';
    case 404:
      return 'Not found';
    case 500:
    case 502:
    case 503:
    case 504:
      return 'Server error';
    default:
      return 'Something went wrong';
  }
}
