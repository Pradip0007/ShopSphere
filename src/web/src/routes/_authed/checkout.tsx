import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createFileRoute } from '@tanstack/react-router';
import { useEffect, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';

import { cartQueryOptions } from '@/features/cart/queries';
import { AddressFields } from '@/features/checkout/AddressFields';
import { submitCheckout } from '@/features/checkout/api';
import { PaymentStep } from '@/features/checkout/PaymentStep';
import { ReviewStep } from '@/features/checkout/ReviewStep';
import { type CheckoutValues, checkoutSchema } from '@/features/checkout/schema';
import { WizardSteps } from '@/features/checkout/WizardSteps';
import { Button } from '@/shared/ui';

export const Route = createFileRoute('/_authed/checkout')({
  component: CheckoutPage,
});

const STEPS = ['Address', 'Payment', 'Review'] as const;

function CheckoutPage(): React.JSX.Element {
  const cart = useQuery(cartQueryOptions());
  const queryClient = useQueryClient();

  const [step, setStep] = useState(0);
  const [paymentMethodId, setPaymentMethodId] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const form = useForm<CheckoutValues>({
    resolver: zodResolver(checkoutSchema),
    defaultValues: {
      shipping: {
        fullName: '',
        line1: '',
        line2: '',
        city: '',
        postalCode: '',
        country: '',
        phone: '',
      },
      billingSameAsShipping: true,
      billing: undefined,
      items: [],
      agreedToTerms: false as unknown as true,
    },
    mode: 'onBlur',
  });

  const {
    register,
    control,
    reset,
    trigger,
    handleSubmit,
    formState: { errors },
  } = form;

  const { fields } = useFieldArray({
    control,
    name: 'items',
  });

  const submitMutation = useMutation({
    mutationFn: submitCheckout,
    onSuccess: async (response) => {
      await queryClient.invalidateQueries({
        queryKey: ['cart'],
      });

      window.location.href = `/orders/${response.orderId}`;
    },
    onError: (error) => {
      setSubmitError(error.message);
    },
  });

  useEffect(() => {
    if (!cart.data) {
      return;
    }

    reset((previous) => ({
      ...previous,
      items: cart.data.lines.map((line) => ({
        lineId: line.productId,
        productId: line.productId,
        quantity: line.quantity,
      })),
    }));
  }, [cart.data, reset]);

  async function goNext(): Promise<void> {
    if (step === 0) {
      const valid = await trigger(['shipping']);

      if (valid) {
        setSubmitError(null);
        setStep(1);
      }

      return;
    }

    if (step === 1) {
      if (!paymentMethodId) {
        setSubmitError('Enter card details first');
        return;
      }

      setSubmitError(null);
      setStep(2);
    }
  }

  async function onSubmit(values: CheckoutValues): Promise<void> {
    setSubmitError(null);

    submitMutation.mutate({
      shippingAddress: {
        line1: values.shipping.line1,
        ...(values.shipping.line2 !== undefined && values.shipping.line2 !== ''
          ? { line2: values.shipping.line2 }
          : {}),
        city: values.shipping.city,
        postalCode: values.shipping.postalCode,
        country: values.shipping.country,
      },
    });
  }

  if (cart.isPending) {
    return <p>Loading cart…</p>;
  }

  if (cart.isError || !cart.data || cart.data.lines.length === 0) {
    return (
      <section className="grid gap-3">
        <h1 className="text-2xl font-semibold">Checkout</h1>

        <p>Your cart is empty.</p>
      </section>
    );
  }

  return (
    <section className="grid max-w-3xl gap-6">
      <h1 className="text-2xl font-semibold">Checkout</h1>

      <WizardSteps steps={[...STEPS]} current={step} />

      <form
        onSubmit={(event) => {
          void handleSubmit(onSubmit)(event);
        }}
        className="grid gap-6"
      >
        {step === 0 && (
          <div className="grid gap-4">
            <h2 className="text-lg font-semibold">Shipping address</h2>

            <AddressFields base="shipping" register={register} errors={errors} />

            <div className="flex justify-end">
              <Button type="button" onClick={() => void goNext()}>
                Continue
              </Button>
            </div>
          </div>
        )}

        {step === 1 && (
          <div className="grid gap-4">
            <h2 className="text-lg font-semibold">Payment</h2>

            <PaymentStep
              onPaymentMethodReady={(id) => {
                setPaymentMethodId(id);
                setSubmitError(null);
              }}
              submitError={submitError}
            />

            {paymentMethodId && (
              <p className="text-sm text-[var(--color-success)]">Card ready. Continue to review.</p>
            )}

            <div className="flex justify-between">
              <Button
                type="button"
                variant="secondary"
                onClick={() => {
                  setSubmitError(null);
                  setStep(0);
                }}
              >
                Back
              </Button>

              <Button type="button" onClick={() => void goNext()}>
                Continue
              </Button>
            </div>
          </div>
        )}

        {step === 2 && (
          <ReviewStep
            register={register}
            errors={errors}
            onBack={() => {
              setSubmitError(null);
              setStep(1);
            }}
            isSubmitting={submitMutation.isPending}
            submitError={submitError}
          />
        )}
      </form>

      <p className="text-sm text-[var(--color-text-muted)]">
        {fields.length} cart item{fields.length === 1 ? '' : 's'} loaded.
      </p>
    </section>
  );
}
