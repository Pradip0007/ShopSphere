interface WizardStepsProps {
  steps: string[];
  current: number;
}

export function WizardSteps({ steps, current }: WizardStepsProps): React.JSX.Element {
  return (
    <ol className="flex list-none items-center gap-2 p-0">
      {steps.map((label, index) => {
        const isActive = index === current;
        const isDone = index < current;

        return (
          <li key={label} className="flex items-center gap-2">
            <span
              aria-current={isActive ? 'step' : undefined}
              className={[
                'inline-flex h-8 w-8 items-center justify-center rounded-full text-sm font-semibold',
                isActive
                  ? 'bg-brand-500 text-white'
                  : isDone
                    ? 'bg-brand-100 text-brand-700'
                    : 'bg-[var(--color-surface-muted)] text-[var(--color-text-muted)]',
              ].join(' ')}
            >
              {index + 1}
            </span>

            <span className={isActive ? 'font-semibold' : 'text-[var(--color-text-muted)]'}>
              {label}
            </span>

            {index < steps.length - 1 && (
              <span aria-hidden className="mx-2 h-px w-8 bg-[var(--color-border)]" />
            )}
          </li>
        );
      })}
    </ol>
  );
}
