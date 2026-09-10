import type { Meta, StoryObj } from '@storybook/tanstack-react';
import { NewsletterForm } from './NewsletterForm';

const meta = {
  title: 'Features/NewsletterForm',
  component: NewsletterForm,
  parameters: {
    layout: 'centered',
  },
} satisfies Meta<typeof NewsletterForm>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};
