import type { VariantProps } from 'class-variance-authority';
import type { buttonVariants } from './Button';

export type ButtonVariant = NonNullable<VariantProps<typeof buttonVariants>['variant']>;

export type ButtonSize = NonNullable<VariantProps<typeof buttonVariants>['size']>;
