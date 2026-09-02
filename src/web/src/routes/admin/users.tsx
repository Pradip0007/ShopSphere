import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/admin/users')({
  component: AdminUsers,
});

function AdminUsers(): React.JSX.Element {
  return (
    <section>
      <h1>Admin · Users</h1>
      <p style={{ opacity: 0.6 }}>Role management lands in Phase 5.</p>
    </section>
  );
}
