import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/')({
  component: HomePage,
});

function HomePage(): React.JSX.Element {
  return (
    <section>
      <h1>ShopSphere</h1>
      <p>Enterprise commerce. React 19 + Vite 6.</p>
    </section>
  );
}
