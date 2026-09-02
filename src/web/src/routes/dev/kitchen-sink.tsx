import { createFileRoute } from '@tanstack/react-router';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from '@/shared/ui/Dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/ui/DropdownMenu';

export const Route = createFileRoute('/dev/kitchen-sink')({
  component: KitchenSink,
});

function KitchenSink(): React.JSX.Element {
  return (
    <section className="grid max-w-2xl gap-6">
      <h1 className="text-2xl font-semibold">Kitchen Sink</h1>

      <div className="flex gap-2">
        <Dialog>
          <DialogTrigger className="rounded-md bg-brand-500 px-4 py-2 text-white hover:bg-brand-600">
            Open dialog
          </DialogTrigger>

          <DialogContent>
            <DialogTitle className="text-lg font-semibold">Confirm action</DialogTitle>

            <DialogDescription className="text-[var(--color-text-muted)]">
              This is a themed Radix Dialog. Focus is trapped inside; press Escape to close.
            </DialogDescription>
          </DialogContent>
        </Dialog>

        <DropdownMenu>
          <DropdownMenuTrigger className="rounded-md border border-[var(--color-border)] px-4 py-2">
            Options ▾
          </DropdownMenuTrigger>

          <DropdownMenuContent>
            <DropdownMenuItem>Edit</DropdownMenuItem>

            <DropdownMenuItem>Duplicate</DropdownMenuItem>

            <DropdownMenuSeparator />

            <DropdownMenuItem className="text-[var(--color-danger)]">Delete</DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="grid gap-3">
        <h2 className="text-xl font-semibold">Token palette</h2>

        <div className="grid grid-cols-10 gap-1">
          {[
            'bg-brand-50',
            'bg-brand-100',
            'bg-brand-200',
            'bg-brand-300',
            'bg-brand-400',
            'bg-brand-500',
            'bg-brand-600',
            'bg-brand-700',
            'bg-brand-800',
            'bg-brand-900',
          ].map((className, index) => (
            <div
              key={className}
              className={`flex aspect-square items-center justify-center rounded text-xs ${className}`}
              style={{
                color: index >= 5 ? 'white' : 'inherit',
              }}
            >
              {(index + 1) * 50}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
