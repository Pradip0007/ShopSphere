import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/products/$slug')({
  component: ProductDetailPage,
});

function ProductDetailPage(): React.JSX.Element {
  const { slug } = Route.useParams();

  return (
    <section>
      <h1>Product: {slug}</h1>
      <p style={{ opacity: 0.6 }}>Detail view lands Day 63.</p>
    </section>
  );
}
