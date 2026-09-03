import type { Preview } from '@storybook/tanstack-react';
import '../src/index.css';

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },

    a11y: {
      test: 'todo',
    },

    backgrounds: {
      default: 'app',
      values: [
        { name: 'app', value: 'var(--color-surface)' },
        { name: 'muted', value: 'var(--color-surface-muted)' },
        { name: 'dark', value: 'oklch(0.16 0 0)' },
      ],
    },
  },
};

export default preview;
