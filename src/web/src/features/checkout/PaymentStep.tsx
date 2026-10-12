import { CardElement, Elements, useElements, useStripe } from '@stripe/react-stripe-js';
import { loadStripe, type Stripe } from '@stripe/stripe-js';
import { useState } from 'react';

const publishableKey = import.meta.env['VITE_STRIPE_PUBLISHABLE_KEY'] as string | undefined;

const stripePromise: Promise<Stripe | null> = publishableKey
  ? loadStripe(publishableKey)
  : Promise.resolve(null);

interface PaymentStepProps {
  onPaymentMethodReady: (id: string) => void;
  submitError: string | null;
}

export function PaymentStep({
  onPaymentMethodReady,
  submitError,
}: PaymentStepProps): React.JSX.Element {
  if (!publishableKey) {
    return (
      <p role="alert" className="text-[var(--color-danger)]">
        Missing VITE_STRIPE_PUBLISHABLE_KEY — check src/web/.env.local.
      </p>
    );
  }

  return (
    <Elements stripe={stripePromise}>
      <PaymentInner onPaymentMethodReady={onPaymentMethodReady} submitError={submitError} />
    </Elements>
  );
}

function PaymentInner({ onPaymentMethodReady, submitError }: PaymentStepProps): React.JSX.Element {
  const stripe = useStripe();
  const elements = useElements();
  const [creating, setCreating] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  async function createPaymentMethod(): Promise<void> {
    if (!stripe || !elements) {
      return;
    }

    const cardElement = elements.getElement(CardElement);

    if (!cardElement) {
      return;
    }

    setCreating(true);
    setLocalError(null);

    try {
      const result = await stripe.createPaymentMethod({
        type: 'card',
        card: cardElement,
      });

      if (result.error) {
        setLocalError(result.error.message ?? 'Could not read card details');
        return;
      }

      onPaymentMethodReady(result.paymentMethod.id);
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className="grid max-w-xl gap-4">
      <div className="grid gap-2">
        <span className="text-sm font-semibold text-slate-800">Card details</span>

        <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
          <div data-testid="stripe-card-element">
            <CardElement options={{ hidePostalCode: true }} />
          </div>
        </div>
      </div>

      {(localError || submitError) && (
        <p role="alert" className="text-sm text-[var(--color-danger)]">
          {localError ?? submitError}
        </p>
      )}

      <button
        type="button"
        data-testid="payment-use-card"
        onClick={() => void createPaymentMethod()}
        disabled={creating || !stripe || !elements}
        className="min-h-12 w-full rounded-lg bg-blue-600 px-4 py-3 font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {creating ? 'Reading card…' : 'Use this card'}
      </button>
    </div>
  );
}
