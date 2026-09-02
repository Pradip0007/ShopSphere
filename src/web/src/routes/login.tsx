import { createFileRoute } from '@tanstack/react-router';
import { type FormEvent, useState } from 'react';
import { z } from 'zod';
import { login } from '@/features/auth/api';
import { router } from '@/router';
import { getUserFromAccessToken } from '@/shared/lib/jwt';
import { store } from '@/store';
import { setCredentials } from '@/store/auth.slice';

const loginSearchSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute('/login')({
  validateSearch: loginSearchSchema,
  component: LoginPage,
});

function LoginPage(): React.JSX.Element {
  const { redirect } = Route.useSearch();

  const [email, setEmail] = useState('smoketest2026@gmail.com');
  const [password, setPassword] = useState('SmokeTest@12345');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    setError(null);
    setIsSubmitting(true);

    try {
      const response = await login({
        email,
        password,
      });

      const user = getUserFromAccessToken(response.accessToken);

      store.dispatch(
        setCredentials({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user,
        }),
      );

      if (redirect) {
        await router.navigate({
          to: redirect,
        });
      } else {
        await router.navigate({
          to: '/',
        });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to sign in.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section
      style={{
        maxWidth: '28rem',
        margin: '3rem auto',
      }}
    >
      <h1>Login</h1>

      {redirect && <p style={{ opacity: 0.6 }}>Sign in to continue to {redirect}</p>}

      <form
        onSubmit={handleSubmit}
        style={{
          display: 'grid',
          gap: '1rem',
          marginTop: '1.5rem',
        }}
      >
        <label
          style={{
            display: 'grid',
            gap: '0.35rem',
          }}
        >
          <span>Email</span>

          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            autoComplete="email"
            required
          />
        </label>

        <label
          style={{
            display: 'grid',
            gap: '0.35rem',
          }}
        >
          <span>Password</span>

          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            required
          />
        </label>

        {error && <p role="alert">{error}</p>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </section>
  );
}
