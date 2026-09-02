import type { FieldValues, Path, UseFormSetError } from 'react-hook-form';
import { ApiError } from '@/shared/lib/api-error';

export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
): string | null {
  if (!(error instanceof ApiError)) {
    return 'Unexpected error';
  }

  const fieldErrors = error.fieldErrors;
  const keys = Object.keys(fieldErrors);

  if (keys.length > 0) {
    for (const key of keys) {
      const normalized = key.charAt(0).toLowerCase() + key.slice(1);

      const message = fieldErrors[key];

      if (message !== undefined) {
        setError(normalized as Path<T>, {
          type: 'server',
          message,
        });
      }
    }

    return null;
  }

  return error.problem?.title ?? error.message ?? 'Request failed';
}
