import { forwardRef, type InputHTMLAttributes } from 'react';
import { cn } from '@/shared/lib/cn';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string | undefined;
}

export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(function FormField(
  { label, error, id, className, ...inputProps },
  ref,
) {
  const inputId = id ?? `field-${label.toLowerCase().replace(/\s+/g, '-')}`;

  const errorId = `${inputId}-error`;

  return (
    <div className="grid gap-1">
      <label htmlFor={inputId} className="text-sm font-medium">
        {label}
      </label>

      <input
        {...inputProps}
        id={inputId}
        ref={ref}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={error ? errorId : undefined}
        className={cn(
          'h-10 rounded-md border px-3 outline-none',
          'bg-[var(--color-surface)] text-[var(--color-text)]',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
          error ? 'border-[var(--color-danger)]' : 'border-[var(--color-border)]',
          className,
        )}
      />

      {error && (
        <span id={errorId} role="alert" className="text-sm text-[var(--color-danger)]">
          {error}
        </span>
      )}
    </div>
  );
});
