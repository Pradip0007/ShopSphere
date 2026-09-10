import { createFileRoute } from '@tanstack/react-router';
import { Suspense } from 'react';
import { AppError } from '@/app/AppError';
import { ProductDetail } from '@/features/products/ProductDetail';
import { ProductDetailSkeleton } from '@/features/products/ProductDetailSkeleton';
import { productDetailQueryOptions } from '@/features/products/queries';

export const Route = createFileRoute('/products/$slug')({
  loader: ({ context, params }) =>
    context.queryClient.ensureQueryData(productDetailQueryOptions(params.slug)),
  pendingComponent: ProductDetailSkeleton,
  pendingMs: 200,
  errorComponent: ({ error, reset }) => <AppError error={error} resetErrorBoundary={reset} />,
  component: ProductDetailPage,
});

function ProductDetailPage(): React.JSX.Element {
  const { slug } = Route.useParams();

  return (
    <Suspense fallback={<ProductDetailSkeleton />}>
      <ProductDetail slug={slug} />
    </Suspense>
  );
}
