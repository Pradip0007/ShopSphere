interface WizardStepsProps {
  steps: string[];
  current: number;
}

export function WizardSteps({ steps, current }: WizardStepsProps): React.JSX.Element {
  return (
    <ol aria-label="Checkout progress" className="flex list-none items-center gap-2 p-0">
      {steps.map((label, index) => {
        const isActive = index === current;
        const isDone = index < current;

        return (
          <li key={label} className="flex items-center gap-2">
            <span
              aria-current={isActive ? 'step' : undefined}
              className={[
                'inline-flex h-9 w-9 items-center justify-center rounded-full text-sm font-semibold transition-colors',
                isActive
                  ? 'bg-blue-600 text-white shadow-sm shadow-blue-200'
                  : isDone
                    ? 'bg-blue-100 text-blue-700'
                    : 'bg-slate-100 text-slate-500',
              ].join(' ')}
            >
              {index + 1}
            </span>

            <span className={isActive ? 'font-semibold text-slate-900' : 'text-slate-500'}>
              {label}
            </span>

            {index < steps.length - 1 && (
              <span aria-hidden className="mx-2 h-px w-8 bg-slate-200 sm:w-16" />
            )}
          </li>
        );
      })}
    </ol>
  );
}
