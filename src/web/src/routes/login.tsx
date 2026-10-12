import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { login } from '@/features/auth/api';
import { applyServerErrors } from '@/features/auth/apply-server-errors';
import { type LoginFormValues, loginSchema } from '@/features/auth/schemas';
import { CART_QUERY_KEY } from '@/features/cart/queries';
import { getUserFromAccessToken } from '@/shared/lib/jwt';
import { Button } from '@/shared/ui/Button';
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
  const queryClient = useQueryClient();
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

      await queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY });

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
    <section className="mx-auto w-full max-w-[472px] rounded-lg border border-slate-300 p-7 shadow-sm sm:p-9">
      <h1 className="mb-5 text-3xl font-medium leading-tight text-slate-950">
        Sign in or create an account
      </h1>

      {redirect && <p className="mb-4 text-sm text-slate-600">Sign in to continue to {redirect}</p>}

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="grid gap-4">
        <FormField
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
          className="h-11 rounded-sm border-slate-400 bg-white"
        />

        <FormField
          label="Password"
          type="password"
          autoComplete="current-password"
          {...register('password')}
          error={errors.password?.message}
          className="h-11 rounded-sm border-slate-400 bg-white"
        />

        {submitError && (
          <div role="alert" className="text-sm text-[var(--color-danger)]">
            {submitError}
          </div>
        )}

        <Button
          type="submit"
          disabled={isSubmitting}
          block
          className="h-11 rounded-full bg-amber-400 text-slate-950 hover:bg-amber-300"
        >
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </Button>
      </form>

      <p className="mt-5 text-sm leading-5 text-slate-800">
        By continuing, you agree to ShopSphere&apos;s{' '}
        <span className="text-sky-700 underline">Conditions of Use</span> and{' '}
        <span className="text-sky-700 underline">Privacy Notice</span>.
      </p>
      <a
        href="mailto:support@shopsphere.local"
        className="mt-5 inline-block text-sky-700 hover:underline"
      >
        Need help?
      </a>

      <div className="mt-7 border-t border-slate-300 pt-5">
        <p className="font-semibold text-slate-950">New to ShopSphere?</p>
        <Link to="/register" className="mt-2 inline-block text-sky-700 hover:underline">
          Create your ShopSphere account
        </Link>
      </div>
    </section>
  );
}
