import { useQuery } from '@tanstack/react-query';
import type { FieldErrors, UseFormRegister } from 'react-hook-form';

import { Button } from '@/shared/ui';
import { checkoutReviewQueryOptions } from './review-queries';
import type { CheckoutValues } from './schema';

interface ReviewStepProps {
  register: UseFormRegister<CheckoutValues>;
  errors: FieldErrors<CheckoutValues>;
  onBack: () => void;
  isSubmitting: boolean;
  submitError: string | null;
}

export function ReviewStep({
  register,
  errors,
  onBack,
  isSubmitting,
  submitError,
}: ReviewStepProps): React.JSX.Element {
  const review = useQuery(checkoutReviewQueryOptions());

  if (review.isPending) {
    return <p>Loading order review…</p>;
  }

  if (review.isError) {
    return (
      <div className="grid gap-4">
        <p role="alert" className="text-[var(--color-danger)]">
          Could not load the order review: {review.error.message}
        </p>

        <Button type="button" variant="secondary" onClick={() => void review.refetch()}>
          Try again
        </Button>
      </div>
    );
  }

  return (
    <div className="grid gap-6">
      <div className="grid max-w-4xl gap-3">
        <h2 className="text-lg font-semibold">Review</h2>

        <ul className="list-none divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white p-0">
          {review.data.items.map((item) => (
            <li key={item.productId} className="flex items-start justify-between gap-4 p-5">
              <div className="grid gap-1">
                <span className="font-medium">
                  {item.productName} × {item.quantity}
                </span>

                <span className="text-sm text-[var(--color-text-muted)]">
                  {formatMoney(item.unitPrice, item.currency)} each
                </span>
              </div>

              <span className="font-medium">{formatMoney(item.lineTotal, item.currency)}</span>
            </li>
          ))}

          <li className="flex items-center justify-between bg-slate-50 p-5 text-lg font-semibold">
            <span>Subtotal</span>

            <span>{formatMoney(review.data.subtotal, review.data.currency)}</span>
          </li>
        </ul>
      </div>

      <div className="grid max-w-4xl gap-2">
        <label className="flex items-start gap-2 text-slate-700">
          <input type="checkbox" {...register('agreedToTerms')} />

          <span>I agree to the terms of sale.</span>
        </label>

        {errors.agreedToTerms && (
          <p role="alert" className="text-sm text-[var(--color-danger)]">
            {String(errors.agreedToTerms.message ?? '')}
          </p>
        )}
        {submitError && (
          <p role="alert" className="text-sm text-[var(--color-danger)]">
            {submitError}
          </p>
        )}
      </div>

      <div className="flex max-w-4xl justify-between border-t border-slate-100 pt-5">
        <Button type="button" variant="secondary" onClick={onBack}>
          Back
        </Button>

        <Button
          type="submit"
          size="lg"
          className="min-w-36 bg-blue-700 text-white hover:bg-blue-800"
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Placing order…' : 'Place order'}
        </Button>
      </div>
    </div>
  );
}

function formatMoney(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency,
  }).format(amount);
}
