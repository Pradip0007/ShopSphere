import { queryOptions } from '@tanstack/react-query';

import { fetchCheckoutReview } from './review-api';

export const CHECKOUT_REVIEW_QUERY_KEY = ['checkout', 'review'] as const;

export const checkoutReviewQueryOptions = () =>
  queryOptions({
    queryKey: CHECKOUT_REVIEW_QUERY_KEY,
    queryFn: fetchCheckoutReview,
    staleTime: 30_000,
  });
