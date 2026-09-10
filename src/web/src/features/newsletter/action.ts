import { z } from 'zod';
import { ApiError } from '@/shared/lib/api-error';
import { subscribeToNewsletter } from './api';

export interface NewsletterState {
  status: 'idle' | 'success' | 'error';
  message: string | null;
  fieldErrors: Record<string, string>;
}

export const initialNewsletterState: NewsletterState = {
  status: 'idle',
  message: null,
  fieldErrors: {},
};

const emailSchema = z.string().min(1, 'Email is required').email('Enter a valid email');

export async function submitNewsletter(
  _prev: NewsletterState,
  formData: FormData,
): Promise<NewsletterState> {
  const raw = formData.get('email');
  const email = typeof raw === 'string' ? raw : '';

  const parsed = emailSchema.safeParse(email);

  if (!parsed.success) {
    return {
      status: 'error',
      message: null,
      fieldErrors: {
        email: parsed.error.issues[0]?.message ?? 'Invalid email',
      },
    };
  }

  try {
    const res = await subscribeToNewsletter({ email: parsed.data });

    return {
      status: 'success',
      message: res.message || 'Thanks — check your inbox to confirm.',
      fieldErrors: {},
    };
  } catch (err) {
    if (err instanceof ApiError) {
      const fieldErrors = err.fieldErrors;

      if (Object.keys(fieldErrors).length > 0) {
        return {
          status: 'error',
          message: null,
          fieldErrors,
        };
      }

      return {
        status: 'error',
        message: err.problem?.title ?? err.message ?? 'Subscription failed',
        fieldErrors: {},
      };
    }

    return {
      status: 'error',
      message: 'Unexpected error. Please try again.',
      fieldErrors: {},
    };
  }
}
