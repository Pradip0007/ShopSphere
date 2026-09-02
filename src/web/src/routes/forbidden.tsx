import { createFileRoute, Link } from '@tanstack/react-router';

export const Route = createFileRoute('/forbidden')({
  component: ForbiddenPage,
});

function ForbiddenPage(): React.JSX.Element {
  return (
    <section
      style={{
        textAlign: 'center',
        padding: '4rem 1rem',
      }}
    >
      <h1>403 — Forbidden</h1>

      <p>You're signed in, but you don't have permission to view this page.</p>

      <Link to="/">Return home</Link>
    </section>
  );
}
