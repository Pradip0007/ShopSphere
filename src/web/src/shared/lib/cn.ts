import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Join Tailwind classes and resolve conflicts.
 * Later classes win: cn('px-2', 'px-4') -> 'px-4'.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
