import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createFileRoute, useNavigate } from '@tanstack/react-router';
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
  const navigate = useNavigate();

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

      await navigate({
        to: '/orders/$id',
        params: { id: response.orderId },
        search: { placed: true },
      });
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

    if (!paymentMethodId) {
      setSubmitError('Enter card details first');
      return;
    }

    submitMutation.mutate({
      paymentMethodId,
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
    return (
      <section className="mx-auto grid max-w-6xl gap-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
        <h1 className="text-2xl font-semibold text-slate-950">Checkout</h1>
        <p className="text-slate-500">Loading cart…</p>
      </section>
    );
  }

  if (cart.isError || !cart.data || cart.data.lines.length === 0) {
    return (
      <section className="mx-auto grid max-w-6xl gap-3 rounded-xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
        <h1 className="text-2xl font-semibold text-slate-950">Checkout</h1>

        <p className="text-slate-600">Your cart is empty.</p>
      </section>
    );
  }

  return (
    <section className="mx-auto grid max-w-6xl gap-7 rounded-xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
      <header className="grid gap-6 border-b border-slate-100 pb-6">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.14em] text-blue-700">
              Secure checkout
            </p>
            <h1 className="mt-1 text-3xl font-bold tracking-tight text-slate-950">Checkout</h1>
          </div>
          <p className="text-sm text-slate-500">
            {fields.length} item{fields.length === 1 ? '' : 's'} in your order
          </p>
        </div>

        <WizardSteps steps={[...STEPS]} current={step} />
      </header>

      <form
        data-testid="checkout-form"
        onSubmit={(event) => {
          void handleSubmit(onSubmit)(event);
        }}
        className="grid gap-7"
      >
        {step === 0 && (
          <div className="grid max-w-4xl gap-6">
            <div>
              <h2 className="text-xl font-semibold text-slate-900">Shipping address</h2>
              <p className="mt-1 text-sm text-slate-500">
                Enter the address where you would like your order delivered.
              </p>
            </div>

            <AddressFields base="shipping" register={register} errors={errors} />

            <div className="flex justify-end border-t border-slate-100 pt-5">
              <Button
                type="button"
                size="lg"
                className="min-w-32 bg-blue-700 text-white hover:bg-blue-800"
                onClick={() => void goNext()}
              >
                Continue
              </Button>
            </div>
          </div>
        )}

        {step === 1 && (
          <div className="grid max-w-2xl gap-6">
            <div>
              <h2 className="text-xl font-semibold text-slate-900">Payment</h2>
              <p className="mt-1 text-sm text-slate-500">
                Your payment information is encrypted and securely processed.
              </p>
            </div>

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
                className="min-w-24"
                onClick={() => {
                  setSubmitError(null);
                  setStep(0);
                }}
              >
                Back
              </Button>

              <Button
                type="button"
                size="lg"
                className="min-w-32 bg-blue-700 text-white hover:bg-blue-800"
                onClick={() => void goNext()}
              >
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

      <p className="flex items-center gap-2 border-t border-slate-100 pt-5 text-sm text-slate-500">
        <span aria-hidden="true" className="text-emerald-600">
          ●
        </span>
        Your order is protected with secure checkout.
      </p>
    </section>
  );
}
