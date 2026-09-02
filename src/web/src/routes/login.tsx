import { zodResolver } from '@hookform/resolvers/zod';
import { createFileRoute, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { login } from '@/features/auth/api';
import { applyServerErrors } from '@/features/auth/apply-server-errors';
import { type LoginFormValues, loginSchema } from '@/features/auth/schemas';
import { getUserFromAccessToken } from '@/shared/lib/jwt';
import { FormField } from '@/shared/ui/FormField';
import { setCredentials } from '@/store/auth.slice';
import { useAppDispatch } from '@/store/hooks';

const loginSearchSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute('/login')({
  validateSearch: loginSearchSchema,
  component: LoginPage,
});

function LoginPage(): React.JSX.Element {
  const { redirect } = Route.useSearch();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  async function onSubmit(values: LoginFormValues): Promise<void> {
    setSubmitError(null);

    try {
      const response = await login(values);

      const user = getUserFromAccessToken(response.accessToken);

      dispatch(
        setCredentials({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user,
        }),
      );

      await navigate({
        to: redirect ?? '/',
      });
    } catch (error) {
      const generic = applyServerErrors(error, setError);

      if (generic) {
        setSubmitError(generic);
      }
    }
  }

  return (
    <section className="max-w-[400px]">
      <h1>Sign in</h1>

      {redirect && <p className="opacity-60">Sign in to continue to {redirect}</p>}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-6 grid gap-4">
        <FormField
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />

        <FormField
          label="Password"
          type="password"
          autoComplete="current-password"
          {...register('password')}
          error={errors.password?.message}
        />

        {submitError && (
          <div role="alert" className="text-[var(--color-danger)]">
            {submitError}
          </div>
        )}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </section>
  );
}
