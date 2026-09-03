import type { Meta, StoryObj } from '@storybook/tanstack-react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  createMemoryHistory,
  createRootRoute,
  createRouter,
  RouterProvider,
} from '@tanstack/react-router';
import { ProductCard } from './ProductCard';

const rootRoute = createRootRoute({
  component: () => null,
});

const router = createRouter({
  routeTree: rootRoute,
  history: createMemoryHistory({
    initialEntries: ['/'],
  }),
});

const queryClient = new QueryClient();

const meta = {
  title: 'Features/ProductCard',
  component: ProductCard,
  decorators: [
    (Story) => (
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router as never} />

        <div className="w-60">
          <Story />
        </div>
      </QueryClientProvider>
    ),
  ],
} satisfies Meta<typeof ProductCard>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    product: {
      id: '1',
      slug: 'demo-runner-3',
      title: 'Demo Runner 3',
      sku: 'DEMO-RUNNER-001',
      price: 79.99,
      currency: 'USD',
      categoryId: '00000000-0000-0000-0000-000000000001',
    },
  },
};
