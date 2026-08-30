import { createFileRoute } from '@tanstack/react-router';
import { z } from 'zod';

const loginSearchSchema = z.object({
  redirect: z.string().optional(),
});

export const Route = createFileRoute('/login')({
  validateSearch: loginSearchSchema,
  component: LoginPage,
});

function LoginPage(): React.JSX.Element {
  const { redirect } = Route.useSearch();

  return (
    <section>
      <h1>Login</h1>
      {redirect && <p>Will bounce you back to: {redirect}</p>}
      <p style={{ opacity: 0.6 }}>Form lands Day 56.</p>
    </section>
  );
}
