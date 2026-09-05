import * as DialogPrimitive from '@radix-ui/react-dialog';
import { type ComponentPropsWithoutRef, type ElementRef, forwardRef } from 'react';
import { cn } from '@/shared/lib/cn';

export const Drawer = DialogPrimitive.Root;
export const DrawerTrigger = DialogPrimitive.Trigger;
export const DrawerClose = DialogPrimitive.Close;
export const DrawerTitle = DialogPrimitive.Title;
export const DrawerDescription = DialogPrimitive.Description;

export const DrawerContent = forwardRef<
  ElementRef<typeof DialogPrimitive.Content>,
  ComponentPropsWithoutRef<typeof DialogPrimitive.Content> & {
    side?: 'left' | 'right';
  }
>(function DrawerContent({ children, className, side = 'right', ...props }, ref) {
  const sideClasses =
    side === 'right'
      ? 'right-0 top-0 h-full w-full max-w-md border-l'
      : 'left-0 top-0 h-full w-full max-w-md border-r';

  return (
    <DialogPrimitive.Portal>
      <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm" />

      <DialogPrimitive.Content
        {...props}
        ref={ref}
        className={cn(
          'fixed z-50 flex flex-col',
          'bg-[var(--color-surface)] border-[var(--color-border)]',
          'shadow-xl',
          sideClasses,
          className,
        )}
      >
        {children}
      </DialogPrimitive.Content>
    </DialogPrimitive.Portal>
  );
});
