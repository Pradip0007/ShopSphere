import type { Meta, StoryObj } from '@storybook/tanstack-react';
import { StatusBadge } from './StatusBadge';
import type { OrderStatus } from './types';

const meta = {
  title: 'Features/StatusBadge',
  component: StatusBadge,
  tags: ['autodocs'],
  argTypes: {
    status: {
      control: 'select',
      options: [
        'Pending',
        'Confirmed',
        'Paid',
        'Shipped',
        'Delivered',
        'Cancelled',
        'Refunded',
      ] satisfies OrderStatus[],
    },
  },
} satisfies Meta<typeof StatusBadge>;

export default meta;

type Story = StoryObj<typeof meta>;

export const AllStates: Story = {
  args: {
    status: 'Pending',
  },
  render: () => (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
      {(
        [
          'Pending',
          'Confirmed',
          'Paid',
          'Shipped',
          'Delivered',
          'Cancelled',
          'Refunded',
        ] as OrderStatus[]
      ).map((status) => (
        <StatusBadge key={status} status={status} />
      ))}
    </div>
  ),
};

export const Pending: Story = {
  args: {
    status: 'Pending',
  },
};

export const Shipped: Story = {
  args: {
    status: 'Shipped',
  },
};

export const Delivered: Story = {
  args: {
    status: 'Delivered',
  },
};

export const Cancelled: Story = {
  args: {
    status: 'Cancelled',
  },
};
