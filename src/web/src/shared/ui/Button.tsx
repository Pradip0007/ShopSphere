import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';
import { type ButtonHTMLAttributes, forwardRef } from 'react';
import { cn } from '@/shared/lib/cn';

const buttonVariants = cva(
  [
    'inline-flex items-center justify-center gap-2',
    'rounded-md font-medium',
    'transition-colors',
    'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
    'disabled:cursor-not-allowed disabled:opacity-50',
  ].join(' '),
  {
    variants: {
      variant: {
        primary: 'bg-brand-500 text-white hover:bg-brand-600 active:bg-brand-700',
        secondary:
          'bg-[var(--color-surface-strong)] text-[var(--color-text)] hover:bg-[var(--color-surface-muted)]',
        ghost: 'bg-transparent text-[var(--color-text)] hover:bg-[var(--color-surface-muted)]',
        outline:
          'border border-[var(--color-border)] bg-transparent text-[var(--color-text)] hover:bg-[var(--color-surface-muted)]',
        destructive: 'bg-[var(--color-danger)] text-white hover:brightness-95 active:brightness-90',
        link: 'bg-transparent p-0 text-brand-600 underline-offset-4 hover:underline',
      },
      size: {
        sm: 'h-8 px-3 text-sm',
        md: 'h-10 px-4 text-base',
        lg: 'h-12 px-6 text-lg',
      },
      block: {
        true: 'w-full',
        false: '',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
      block: false,
    },
  },
);

export interface ButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

/**
 * Button — the design-system button.
 *
 * Variants: primary | secondary | ghost | outline | destructive | link
 * Sizes:    sm | md | lg
 * Props:    block, disabled, asChild
 *
 * Storybook: see Button.stories.tsx (Day 60)
 */

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { className, variant, size, block, asChild, type = 'button', ...props },
  ref,
) {
  const Comp = asChild ? Slot : 'button';

  return (
    <Comp
      {...(asChild ? {} : { type })}
      className={cn(
        buttonVariants({
          variant,
          size,
          block,
        }),
        className,
      )}
      ref={ref as never}
      {...props}
    />
  );
});

export { buttonVariants };
