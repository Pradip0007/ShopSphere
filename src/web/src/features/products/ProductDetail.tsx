import { useSuspenseQuery } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { useState } from 'react';
import { useAddToCart } from '@/features/cart/queries';
import { WishlistToggle } from '@/features/wishlist/WishlistToggle';
import { Button, Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui';
import { ProductGallery } from './ProductGallery';
import { ProductReviews } from './ProductReviews';
import { productDetailQueryOptions } from './queries';
import { useStockLevel } from './useStockLevel';

const SAMPLE_HIGHLIGHTS = [
  { title: 'Thoughtful design', description: 'Made for everyday use and lasting value.' },
  { title: 'Quality checked', description: 'Carefully selected for a dependable experience.' },
  { title: 'Easy gifting', description: 'A considered pick for someone special.' },
];

interface ProductDetailProps {
  slug: string;
}

export function ProductDetail({ slug }: ProductDetailProps): React.JSX.Element {
  const { data: product } = useSuspenseQuery(productDetailQueryOptions(slug));
  const stock = useStockLevel(product.sku, product.stock);
  const addToCart = useAddToCart();
  const [quantity, setQuantity] = useState(1);

  const formatPrice = (amount: number): string =>
    new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: product.currency,
    }).format(amount);

  const listPrice = product.price / 0.8;

  return (
    <article className="mx-auto grid w-full max-w-7xl gap-8 pb-10">
      <nav
        aria-label="Breadcrumb"
        className="flex flex-wrap items-center gap-2 text-sm text-slate-500"
      >
        <Link to="/" className="transition hover:text-blue-700 hover:underline">
          Home
        </Link>
        <span aria-hidden="true">/</span>
        <Link to="/products" className="transition hover:text-blue-700 hover:underline">
          Products
        </Link>
        <span aria-hidden="true">/</span>
        <span aria-current="page" className="font-medium text-slate-700">
          {product.title}
        </span>
      </nav>

      <div className="grid items-start gap-8 lg:grid-cols-[minmax(0,1.1fr)_minmax(20rem,0.9fr)] lg:gap-10 xl:gap-12">
        <ProductGallery
          images={product.images}
          productName={product.title}
          category={product.category}
        />

        <section className="grid content-start gap-5 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7">
          <div className="grid gap-3">
            <div className="flex flex-wrap items-center gap-2">
              <span className="rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-blue-700">
                {product.category}
              </span>
              <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                20% sample offer
              </span>
            </div>
            <h1 className="text-3xl font-bold leading-tight tracking-tight text-slate-950 sm:text-4xl">
              {product.title}
            </h1>
            <p className="text-sm text-slate-500">SKU: {product.sku}</p>
          </div>

          <div className="flex flex-wrap items-center gap-x-3 gap-y-1 border-y border-slate-100 py-4">
            {product.averageRating !== null ? (
              <>
                <span className="font-semibold text-slate-900">
                  {product.averageRating.toFixed(1)}
                </span>
                <span
                  role="img"
                  className="tracking-wide text-amber-500"
                  aria-label={`${product.averageRating} out of 5 stars`}
                >
                  {'★'.repeat(Math.round(product.averageRating))}
                  {'☆'.repeat(5 - Math.round(product.averageRating))}
                </span>
                <a
                  href="#product-reviews"
                  className="text-sm font-medium text-blue-700 hover:underline"
                >
                  {product.ratingCount.toLocaleString()} customer reviews
                </a>
              </>
            ) : (
              <span className="text-sm text-slate-600">Be the first to review this product</span>
            )}
            <span aria-hidden="true" className="text-slate-300">
              |
            </span>
            <span className="text-sm font-medium text-emerald-700">
              {stock > 0 ? 'In stock' : 'Currently unavailable'}
            </span>
          </div>

          <div className="grid gap-1">
            <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
              <span
                data-testid="product-detail-price"
                className="text-3xl font-bold text-slate-950"
              >
                {formatPrice(product.price)}
              </span>
              <span className="text-base text-slate-500">
                <span className="sr-only">Previous price </span>
                <s>{formatPrice(listPrice)}</s>
              </span>
              <span className="rounded bg-emerald-50 px-2 py-1 text-sm font-semibold text-emerald-700">
                Save 20%
              </span>
            </div>
            <p className="text-xs text-slate-500">
              Illustrative offer · Price shown is the current product price.
            </p>
            <p className="mt-1 text-sm font-medium text-slate-700">Inclusive of applicable taxes</p>
          </div>

          <div className="rounded-xl bg-slate-50 p-4">
            <p className="text-sm font-semibold text-slate-900">About this item</p>
            <p className="mt-2 whitespace-pre-line text-sm leading-6 text-slate-600">
              {product.longDescription}
            </p>
          </div>

          <div className="grid gap-3">
            <div className="flex flex-wrap items-center gap-3">
              <label className="flex h-12 items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-sm font-medium text-slate-700">
                Quantity
                <select
                  aria-label="Quantity"
                  value={quantity}
                  onChange={(event) => setQuantity(Number(event.target.value))}
                  className="bg-transparent font-semibold text-slate-900 outline-none"
                >
                  {Array.from(
                    { length: Math.min(Math.max(stock, 1), 10) },
                    (_, index) => index + 1,
                  ).map((value) => (
                    <option key={value} value={value}>
                      {value}
                    </option>
                  ))}
                </select>
              </label>
              {stock > 0 && stock <= 5 && (
                <span className="text-sm font-medium text-amber-700">Only {stock} left</span>
              )}
            </div>

            <div className="flex flex-wrap gap-3">
              <Button
                size="lg"
                className="min-h-12 flex-1 rounded-lg bg-blue-700 font-semibold text-white shadow-sm hover:bg-blue-800 active:bg-blue-900"
                data-testid="product-detail-add-to-cart"
                disabled={stock <= 0 || addToCart.isPending}
                onClick={() => {
                  addToCart.mutate({
                    productId: product.id,
                    quantity,
                  });
                }}
              >
                {stock <= 0
                  ? 'Out of stock'
                  : addToCart.isPending
                    ? 'Adding to cart…'
                    : 'Add to cart'}
              </Button>
              <WishlistToggle
                productId={product.id}
                className="size-12 border border-slate-200 bg-white text-slate-600 hover:border-pink-200 hover:bg-pink-50"
              />
            </div>
            {addToCart.isSuccess && (
              <p role="status" className="text-sm font-medium text-emerald-700">
                Added to your cart.
              </p>
            )}
            {addToCart.isError && (
              <p role="alert" className="text-sm font-medium text-red-700">
                Could not add this product to your cart. Please try again.
              </p>
            )}
          </div>

          <div className="grid gap-3 border-t border-slate-100 pt-5 sm:grid-cols-2">
            <Benefit icon="✓" title="Secure checkout" description="Protected payment experience" />
            <Benefit
              icon="↗"
              title="Delivery information"
              description="See details before you order"
            />
          </div>
        </section>
      </div>

      <section className="grid gap-5 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7">
        <Tabs defaultValue="description">
          <TabsList aria-label="Product information" className="flex-wrap gap-2 border-slate-200">
            <TabsTrigger
              value="description"
              className="rounded-t-lg px-5 py-3 text-slate-600 data-[state=active]:border-blue-600 data-[state=active]:text-blue-700"
            >
              Description
            </TabsTrigger>
            <TabsTrigger
              id="product-reviews"
              value="reviews"
              className="rounded-t-lg px-5 py-3 text-slate-600 data-[state=active]:border-blue-600 data-[state=active]:text-blue-700"
            >
              Reviews {product.ratingCount > 0 ? `(${product.ratingCount})` : ''}
            </TabsTrigger>
            <TabsTrigger
              value="shipping"
              className="rounded-t-lg px-5 py-3 text-slate-600 data-[state=active]:border-blue-600 data-[state=active]:text-blue-700"
            >
              Shipping
            </TabsTrigger>
          </TabsList>
          <TabsContent value="description" className="max-w-4xl">
            <h2 className="text-lg font-semibold text-slate-900">Product details</h2>
            <p className="mt-2 whitespace-pre-line text-sm leading-7 text-slate-600">
              {product.longDescription}
            </p>
            <div className="mt-6">
              <h3 className="font-semibold text-slate-900">
                {product.attributes.length > 0 ? 'Specifications' : 'Sample product highlights'}
              </h3>
              {product.attributes.length > 0 ? (
                <dl className="mt-3 grid overflow-hidden rounded-xl border border-slate-200 sm:grid-cols-2">
                  {product.attributes.map((attribute) => (
                    <div
                      key={attribute.name}
                      className="grid grid-cols-[minmax(0,0.8fr)_minmax(0,1.2fr)] gap-4 border-b border-slate-100 px-4 py-3 text-sm even:bg-slate-50"
                    >
                      <dt className="text-slate-500">{attribute.name}</dt>
                      <dd className="font-medium text-slate-800">{attribute.value}</dd>
                    </div>
                  ))}
                </dl>
              ) : (
                <>
                  <p className="mt-1 text-xs text-slate-500">
                    Sample copy for preview only; specifications have not been provided.
                  </p>
                  <div className="mt-3 grid gap-3 md:grid-cols-3">
                    {SAMPLE_HIGHLIGHTS.map((highlight) => (
                      <div
                        key={highlight.title}
                        className="rounded-xl border border-slate-200 bg-slate-50 p-4"
                      >
                        <h4 className="text-sm font-semibold text-slate-800">{highlight.title}</h4>
                        <p className="mt-1 text-sm leading-5 text-slate-600">
                          {highlight.description}
                        </p>
                      </div>
                    ))}
                  </div>
                </>
              )}
            </div>
          </TabsContent>
          <TabsContent value="reviews">
            <ProductReviews slug={slug} />
          </TabsContent>
          <TabsContent value="shipping">
            <div className="grid max-w-3xl gap-3">
              <h2 className="text-lg font-semibold text-slate-900">Shipping & returns</h2>
              <p className="whitespace-pre-line text-sm leading-7 text-slate-600">
                {product.shippingInfo}
              </p>
              <p className="rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-800">
                Delivery dates and return eligibility are confirmed for your location during
                checkout.
              </p>
            </div>
          </TabsContent>
        </Tabs>
      </section>
    </article>
  );
}

function Benefit({
  icon,
  title,
  description,
}: {
  icon: string;
  title: string;
  description: string;
}): React.JSX.Element {
  return (
    <div className="flex items-start gap-3">
      <span
        aria-hidden="true"
        className="grid size-9 shrink-0 place-items-center rounded-full bg-emerald-50 font-bold text-emerald-700"
      >
        {icon}
      </span>
      <span className="grid gap-0.5">
        <span className="text-sm font-semibold text-slate-800">{title}</span>
        <span className="text-xs leading-5 text-slate-500">{description}</span>
      </span>
    </div>
  );
}
