import { useQuery } from '@tanstack/react-query';
import { productReviewsQueryOptions } from './queries';

interface ProductReviewsProps {
  slug: string;
}

export function ProductReviews({ slug }: ProductReviewsProps): React.JSX.Element {
  const query = useQuery(productReviewsQueryOptions(slug));

  if (query.isPending) {
    return (
      <div className="grid gap-3" role="status" aria-label="Loading reviews">
        <div className="h-20 animate-pulse rounded-xl bg-slate-100" />
        <div className="h-28 animate-pulse rounded-xl bg-slate-100" />
      </div>
    );
  }

  if (query.isError) {
    return (
      <div
        role="alert"
        className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-800"
      >
        Could not load reviews. Please try again later.
      </div>
    );
  }

  if (query.data.items.length === 0) {
    return (
      <div className="grid justify-items-start gap-3 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6 sm:p-8">
        <span
          aria-hidden="true"
          className="grid size-11 place-items-center rounded-full bg-amber-100 text-xl text-amber-600"
        >
          ★
        </span>
        <div>
          <h2 className="font-semibold text-slate-900">No reviews yet</h2>
          <p className="mt-1 text-sm text-slate-600">
            Be the first to share your experience with this product.
          </p>
        </div>
        <span className="text-xs text-slate-500">
          Customer reviews will appear here once they are available.
        </span>
      </div>
    );
  }

  const averageRating =
    query.data.items.reduce((total, review) => total + review.rating, 0) / query.data.items.length;

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center gap-5 rounded-xl bg-amber-50 p-5">
        <div className="grid justify-items-center">
          <span className="text-4xl font-bold text-slate-900">{averageRating.toFixed(1)}</span>
          <span
            role="img"
            aria-label={`${averageRating.toFixed(1)} out of 5 stars`}
            className="text-amber-500"
          >
            {renderStars(averageRating)}
          </span>
        </div>
        <div>
          <p className="font-semibold text-slate-900">Customer feedback</p>
          <p className="text-sm text-slate-600">
            Based on {query.data.items.length} review{query.data.items.length === 1 ? '' : 's'}
          </p>
        </div>
      </div>
      <ul className="grid list-none gap-3 p-0">
        {query.data.items.map((review) => (
          <li key={review.id} className="rounded-xl border border-slate-200 bg-white p-5">
            <header className="flex items-baseline justify-between gap-4">
              <strong className="text-slate-900">{review.authorDisplayName}</strong>
              <span className="text-sm text-[var(--color-text-muted)]">
                {new Date(review.createdUtc).toLocaleDateString()}
              </span>
            </header>
            <div className="mt-2 flex items-center gap-2">
              <span
                role="img"
                aria-label={`${review.rating} out of 5 stars`}
                className="text-amber-500"
              >
                {renderStars(review.rating)}
              </span>
              {review.title && <strong className="text-sm text-slate-800">{review.title}</strong>}
            </div>
            <p className="mt-2 leading-6 text-slate-600">{review.body}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}

function renderStars(rating: number): string {
  const full = Math.max(0, Math.min(5, Math.round(rating)));
  return '★'.repeat(full) + '☆'.repeat(5 - full);
}
