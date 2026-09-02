import * as Menu from '@radix-ui/react-dropdown-menu';
import { type ComponentPropsWithoutRef, type ElementRef, forwardRef } from 'react';

export const DropdownMenu = Menu.Root;
export const DropdownMenuTrigger = Menu.Trigger;
export const DropdownMenuGroup = Menu.Group;

export const DropdownMenuSeparator = forwardRef<
  ElementRef<typeof Menu.Separator>,
  ComponentPropsWithoutRef<typeof Menu.Separator>
>(function DropdownMenuSeparator(props, ref) {
  return <Menu.Separator {...props} ref={ref} className="my-1 h-px bg-[var(--color-border)]" />;
});

export const DropdownMenuContent = forwardRef<
  ElementRef<typeof Menu.Content>,
  ComponentPropsWithoutRef<typeof Menu.Content>
>(function DropdownMenuContent({ sideOffset = 6, ...props }, ref) {
  return (
    <Menu.Portal>
      <Menu.Content
        {...props}
        ref={ref}
        sideOffset={sideOffset}
        className="z-50 min-w-40 rounded-md border p-1 shadow-lg bg-[var(--color-surface)] border-[var(--color-border)]"
      />
    </Menu.Portal>
  );
});

export const DropdownMenuItem = forwardRef<
  ElementRef<typeof Menu.Item>,
  ComponentPropsWithoutRef<typeof Menu.Item>
>(function DropdownMenuItem(props, ref) {
  return (
    <Menu.Item
      {...props}
      ref={ref}
      className="cursor-pointer rounded-sm px-3 py-1.5 text-sm outline-none data-[highlighted]:bg-[var(--color-surface-muted)] data-[disabled]:opacity-50 data-[disabled]:cursor-not-allowed"
    />
  );
});
