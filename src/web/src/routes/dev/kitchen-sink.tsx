import { createFileRoute, Link } from '@tanstack/react-router';
import { Button } from '@/shared/ui/Button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from '@/shared/ui/Dialog';

export const Route = createFileRoute('/dev/kitchen-sink')({
  component: KitchenSink,
});

function KitchenSink(): React.JSX.Element {
  return (
    <section className="grid max-w-3xl gap-8">
      <h1 className="text-2xl font-semibold">Kitchen Sink</h1>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">Button variants</h2>

        <div className="flex flex-wrap gap-3">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="destructive">Destructive</Button>
          <Button variant="link">Link</Button>
        </div>
      </div>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">Button sizes</h2>

        <div className="flex flex-wrap items-center gap-3">
          <Button size="sm">Small</Button>
          <Button size="md">Medium</Button>
          <Button size="lg">Large</Button>
        </div>
      </div>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">Block button</h2>

        <Button block variant="primary">
          Full width
        </Button>
      </div>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">Disabled</h2>

        <div className="flex gap-3">
          <Button disabled>Disabled primary</Button>
          <Button variant="destructive" disabled>
            Disabled destructive
          </Button>
        </div>
      </div>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">asChild — Button as Link</h2>

        <Button asChild variant="outline">
          <Link to="/products">Browse products</Link>
        </Button>
      </div>

      <div className="grid gap-3">
        <h2 className="text-lg font-semibold">Dialog with themed buttons</h2>

        <Dialog>
          <DialogTrigger asChild>
            <Button variant="primary">Delete account…</Button>
          </DialogTrigger>

          <DialogContent>
            <DialogTitle className="text-lg font-semibold">Confirm deletion</DialogTitle>

            <DialogDescription className="mt-2 text-[var(--color-text-muted)]">
              This action cannot be undone.
            </DialogDescription>

            <div className="mt-6 flex justify-end gap-2">
              <Button variant="ghost">Cancel</Button>
              <Button variant="destructive">Delete</Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </section>
  );
}
