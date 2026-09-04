import { useQuery } from '@tanstack/react-query';
import { productReviewsQueryOptions } from './queries';

interface ProductReviewsProps {
  slug: string;
}

export function ProductReviews({ slug }: ProductReviewsProps): React.JSX.Element {
  const query = useQuery(productReviewsQueryOptions(slug));

  if (query.isPending) {
    return <p className="text-[var(--color-text-muted)]">Loading reviews…</p>;
  }

  if (query.isError) {
    return (
      <p role="alert" className="text-[var(--color-danger)]">
        Could not load reviews.
      </p>
    );
  }

  if (query.data.items.length === 0) {
    return <p className="text-[var(--color-text-muted)]">No reviews yet. Be the first!</p>;
  }

  return (
    <ul className="grid list-none gap-4 p-0">
      {query.data.items.map((review) => (
        <li key={review.id} className="rounded-lg border border-[var(--color-border)] p-4">
          <header className="flex items-baseline justify-between gap-4">
            <strong>{review.authorDisplayName}</strong>
            <span className="text-sm text-[var(--color-text-muted)]">
              {new Date(review.createdUtc).toLocaleDateString()}
            </span>
          </header>
          <p className="mt-1">
            {renderStars(review.rating)}
            {review.title && ` · ${review.title}`}
          </p>
          <p className="mt-2 text-[var(--color-text-muted)]">{review.body}</p>
        </li>
      ))}
    </ul>
  );
}

function renderStars(rating: number): string {
  const full = Math.max(0, Math.min(5, Math.round(rating)));
  return '★'.repeat(full) + '☆'.repeat(5 - full);
}
