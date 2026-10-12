import { zodResolver } from '@hookform/resolvers/zod';
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { login, register as registerApi } from '@/features/auth/api';
import { applyServerErrors } from '@/features/auth/apply-server-errors';
import { type RegisterFormValues, registerSchema } from '@/features/auth/schemas';
import { getUserFromAccessToken } from '@/shared/lib/jwt';
import { Button } from '@/shared/ui/Button';
import { FormField } from '@/shared/ui/FormField';
import { setCredentials } from '@/store/auth.slice';
import { useAppDispatch } from '@/store/hooks';

export const Route = createFileRoute('/register')({
  component: RegisterPage,
});

function RegisterPage(): React.JSX.Element {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      displayName: '',
      password: '',
      confirmPassword: '',
    },
  });

  async function onSubmit(values: RegisterFormValues): Promise<void> {
    setSubmitError(null);

    try {
      // displayName is intentionally frontend-only for Day 56.
      // The current backend registration contract accepts only
      // email and password.
      await registerApi({
        email: values.email,
        password: values.password,
      });

      // The backend returns only UserId from registration,
      // so authenticate the newly-created user through login.
      const response = await login({
        email: values.email,
        password: values.password,
      });

      const user = getUserFromAccessToken(response.accessToken);

      dispatch(
        setCredentials({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user,
        }),
      );

      await navigate({
        to: '/',
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
      <h1 className="mb-5 text-3xl font-medium leading-tight text-slate-950">Create account</h1>

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
          label="Display name"
          type="text"
          autoComplete="name"
          {...register('displayName')}
          error={errors.displayName?.message}
          className="h-11 rounded-sm border-slate-400 bg-white"
        />

        <FormField
          label="Password"
          type="password"
          autoComplete="new-password"
          {...register('password')}
          error={errors.password?.message}
          className="h-11 rounded-sm border-slate-400 bg-white"
        />

        <FormField
          label="Confirm password"
          type="password"
          autoComplete="new-password"
          {...register('confirmPassword')}
          error={errors.confirmPassword?.message}
          className="h-11 rounded-sm border-slate-400 bg-white"
        />

        {submitError && (
          <div role="alert" className="text-[var(--color-danger)]">
            {submitError}
          </div>
        )}

        <Button
          type="submit"
          disabled={isSubmitting}
          block
          className="h-11 rounded-full bg-amber-400 text-slate-950 hover:bg-amber-300"
        >
          {isSubmitting ? 'Creating…' : 'Create account'}
        </Button>
      </form>
      <p className="mt-5 text-sm leading-5 text-slate-800">
        By creating an account, you agree to ShopSphere&apos;s{' '}
        <span className="text-sky-700 underline">Conditions of Use</span> and{' '}
        <span className="text-sky-700 underline">Privacy Notice</span>.
      </p>
      <div className="mt-7 border-t border-slate-300 pt-5 text-sm">
        <span>Already have an account? </span>
        <Link to="/login" search={{}} className="text-sky-700 hover:underline">
          Sign in
        </Link>
      </div>
    </section>
  );
}
