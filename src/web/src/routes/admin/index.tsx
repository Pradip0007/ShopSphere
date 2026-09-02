import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/admin/')({
  component: AdminHome,
});

function AdminHome(): React.JSX.Element {
  return (
    <section>
      <h1>Admin Dashboard</h1>
      <p>Pick a section from the sidebar.</p>
    </section>
  );
}
