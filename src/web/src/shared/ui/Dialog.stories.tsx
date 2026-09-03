import type { Meta, StoryObj } from '@storybook/tanstack-react';
import { expect, screen, userEvent, within } from 'storybook/test';
import { Button } from './Button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from './Dialog';

const meta = {
  title: 'UI/Dialog',
  component: Dialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
  },
} satisfies Meta<typeof Dialog>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Confirm: Story = {
  render: () => (
    <Dialog>
      <DialogTrigger asChild>
        <Button>Open confirm</Button>
      </DialogTrigger>

      <DialogContent>
        <DialogTitle className="text-lg font-semibold">Delete this order?</DialogTitle>

        <DialogDescription className="mt-1 text-[var(--color-text-muted)]">
          This cannot be undone.
        </DialogDescription>

        <div className="mt-6 flex justify-end gap-2">
          <DialogClose asChild>
            <Button variant="ghost">Cancel</Button>
          </DialogClose>

          <Button variant="destructive">Delete</Button>
        </div>
      </DialogContent>
    </Dialog>
  ),
};

export const KeyboardInteraction: Story = {
  render: () => (
    <Dialog>
      <DialogTrigger asChild>
        <Button>Open</Button>
      </DialogTrigger>

      <DialogContent>
        <DialogTitle>A dialog</DialogTitle>

        <DialogDescription>Press Escape to close.</DialogDescription>
      </DialogContent>
    </Dialog>
  ),

  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await userEvent.click(
      canvas.getByRole('button', {
        name: /open/i,
      }),
    );

    const title = await screen.findByText('A dialog');

    await expect(title).toBeInTheDocument();

    await userEvent.keyboard('{Escape}');

    await expect(screen.queryByText('A dialog')).not.toBeInTheDocument();
  },
};
