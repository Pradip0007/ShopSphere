import { forwardRef, type InputHTMLAttributes } from 'react';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string | undefined;
}

export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(function FormField(
  { label, error, id, ...inputProps },
  ref,
) {
  const inputId = id ?? `field-${label.toLowerCase().replace(/\s+/g, '-')}`;

  const errorId = `${inputId}-error`;

  return (
    <div
      style={{
        display: 'grid',
        gap: 4,
      }}
    >
      <label
        htmlFor={inputId}
        style={{
          fontWeight: 500,
        }}
      >
        {label}
      </label>

      <input
        {...inputProps}
        id={inputId}
        ref={ref}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={error ? errorId : undefined}
        style={{
          padding: '0.5rem 0.75rem',
          border: `1px solid ${error ? '#c00' : '#ccc'}`,
          borderRadius: 6,
          font: 'inherit',
        }}
      />

      {error && (
        <span
          id={errorId}
          role="alert"
          style={{
            color: '#c00',
            fontSize: '0.85rem',
          }}
        >
          {error}
        </span>
      )}
    </div>
  );
});
