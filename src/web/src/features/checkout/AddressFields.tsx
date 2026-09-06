import type { FieldErrors, UseFormRegister } from 'react-hook-form';

import { FormField } from '@/shared/ui';
import type { CheckoutValues } from './schema';

interface AddressFieldsProps {
  base: 'shipping' | 'billing';
  register: UseFormRegister<CheckoutValues>;
  errors: FieldErrors<CheckoutValues>;
}

export function AddressFields({ base, register, errors }: AddressFieldsProps): React.JSX.Element {
  const err = (field: keyof CheckoutValues['shipping']): string | undefined => {
    const branch = (
      errors as unknown as Record<
        string,
        Record<string, { message?: string } | undefined> | undefined
      >
    )[base];

    return branch?.[field]?.message;
  };

  return (
    <div className="grid gap-3 md:grid-cols-2">
      <div className="md:col-span-2">
        <FormField
          label="Full name"
          autoComplete="name"
          {...register(`${base}.fullName` as const)}
          error={err('fullName')}
        />
      </div>

      <div className="md:col-span-2">
        <FormField
          label="Address line 1"
          autoComplete="address-line1"
          {...register(`${base}.line1` as const)}
          error={err('line1')}
        />
      </div>

      <div className="md:col-span-2">
        <FormField
          label="Address line 2 (optional)"
          autoComplete="address-line2"
          {...register(`${base}.line2` as const)}
          error={err('line2')}
        />
      </div>

      <FormField
        label="City"
        autoComplete="address-level2"
        {...register(`${base}.city` as const)}
        error={err('city')}
      />

      <FormField
        label="Postal code"
        autoComplete="postal-code"
        {...register(`${base}.postalCode` as const)}
        error={err('postalCode')}
      />

      <FormField
        label="Country (ISO)"
        autoComplete="country"
        placeholder="IN"
        {...register(`${base}.country` as const)}
        error={err('country')}
      />

      <FormField
        label="Phone (optional)"
        autoComplete="tel"
        {...register(`${base}.phone` as const)}
        error={err('phone')}
      />
    </div>
  );
}
