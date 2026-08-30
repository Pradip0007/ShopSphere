import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/register')({
  component: RegisterPage,
});

function RegisterPage(): React.JSX.Element {
  return (
    <section>
      <h1>Register</h1>
      <p style={{ opacity: 0.6 }}>Form lands Day 56.</p>
    </section>
  );
}
