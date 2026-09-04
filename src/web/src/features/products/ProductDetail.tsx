import { useSuspenseQuery } from '@tanstack/react-query';
import { Button, Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui';
import { ProductGallery } from './ProductGallery';
import { ProductReviews } from './ProductReviews';
import { productDetailQueryOptions } from './queries';

interface ProductDetailProps {
  slug: string;
}

export function ProductDetail({ slug }: ProductDetailProps): React.JSX.Element {
  const { data: product } = useSuspenseQuery(productDetailQueryOptions(slug));
  const price = new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: product.currency,
  }).format(product.price);

  return (
    <article className="grid gap-8">
      <div className="grid gap-8 md:grid-cols-2">
        <ProductGallery images={product.images} productName={product.title} />

        <div className="grid content-start gap-4">
          <div>
            <p className="text-sm uppercase text-[var(--color-text-muted)]">{product.category}</p>
            <h1 className="text-3xl font-semibold">{product.title}</h1>
            {product.averageRating !== null && (
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {product.averageRating.toFixed(1)} ★ ({product.ratingCount} reviews)
              </p>
            )}
          </div>

          <p className="text-2xl font-semibold">{price}</p>
          <p className="text-[var(--color-text-muted)]">{product.longDescription}</p>

          <div className="flex items-center gap-3">
            <Button
              size="lg"
              disabled={product.stock <= 0}
              onClick={() => console.info('Add to cart', product.id)}
            >
              {product.stock > 0 ? 'Add to cart' : 'Out of stock'}
            </Button>
            {product.stock > 0 && product.stock < 5 && (
              <span className="text-sm text-[var(--color-warning)]">Only {product.stock} left</span>
            )}
          </div>

          <dl className="mt-4 grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
            <dt className="text-[var(--color-text-muted)]">SKU</dt>
            <dd>{product.sku}</dd>
            {product.attributes.map((attribute) => (
              <div key={attribute.name} className="contents">
                <dt className="text-[var(--color-text-muted)]">{attribute.name}</dt>
                <dd>{attribute.value}</dd>
              </div>
            ))}
          </dl>
        </div>
      </div>

      <Tabs defaultValue="description">
        <TabsList aria-label="Product information">
          <TabsTrigger value="description">Description</TabsTrigger>
          <TabsTrigger value="reviews">Reviews</TabsTrigger>
          <TabsTrigger value="shipping">Shipping</TabsTrigger>
        </TabsList>
        <TabsContent value="description">
          <p className="max-w-none whitespace-pre-line">{product.longDescription}</p>
        </TabsContent>
        <TabsContent value="reviews">
          <ProductReviews slug={slug} />
        </TabsContent>
        <TabsContent value="shipping">
          <p className="whitespace-pre-line">{product.shippingInfo}</p>
        </TabsContent>
      </Tabs>
    </article>
  );
}
