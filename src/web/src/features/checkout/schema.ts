import { z } from 'zod';

export const addressSchema = z.object({
  fullName: z.string().min(2, 'Enter your full name'),
  line1: z.string().min(3, 'Address line 1 required'),
  line2: z.string().optional(),
  city: z.string().min(2, 'City required'),
  postalCode: z.string().min(3, 'Postal code required'),
  country: z.string().length(2, 'Use a 2-letter country code'),
  phone: z.string().optional(),
});

export const checkoutSchema = z.object({
  shipping: addressSchema,
  billingSameAsShipping: z.boolean(),
  billing: addressSchema.optional(),
  items: z
    .array(
      z.object({
        lineId: z.string(),
        productId: z.string(),
        quantity: z.number().int().positive(),
      }),
    )
    .min(1, 'Cart is empty'),
  agreedToTerms: z.literal(true, {
    error: 'You must agree to the terms',
  }),
});

export type CheckoutValues = z.infer<typeof checkoutSchema>;
export type AddressValues = z.infer<typeof addressSchema>;
