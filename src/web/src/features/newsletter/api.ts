import { apiFetch } from '@/shared/lib/api-fetch';

export interface NewsletterSignupRequest {
  email: string;
}

export interface NewsletterSignupResponse {
  message: string;
}

export function subscribeToNewsletter(
  req: NewsletterSignupRequest,
): Promise<NewsletterSignupResponse> {
  return apiFetch<NewsletterSignupResponse>('/api/v1/newsletter/subscribe', {
    method: 'POST',
    json: req,
    // Newsletter is anonymous — skip the auth header.
    skipAuth: true,
  });
}
