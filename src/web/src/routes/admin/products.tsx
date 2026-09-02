import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/admin/products')({
  component: AdminProducts,
});

function AdminProducts(): React.JSX.Element {
  return (
    <section>
      <h1>Admin · Products</h1>
      <p style={{ opacity: 0.6 }}>CRUD grid lands in Phase 5.</p>
    </section>
  );
}
