import { useEffect, useRef } from 'react';

export interface UseIntersectOptions {
  /** Fires whenever the target enters the viewport. */
  onIntersect: () => void;

  /** Skip observing, e.g. when there is no next page. */
  enabled?: boolean;

  rootMargin?: string;
  threshold?: number;
}

/**
 * Attach the returned ref to a sentinel element at the end of a list.
 * When it scrolls into view, onIntersect fires — perfect for infinite scroll.
 */
export function useIntersect<T extends HTMLElement>({
  onIntersect,
  enabled = true,
  rootMargin = '200px 0px',
  threshold = 0,
}: UseIntersectOptions): (node: T | null) => void {
  const nodeRef = useRef<T | null>(null);
  const cbRef = useRef(onIntersect);

  cbRef.current = onIntersect;

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const node = nodeRef.current;

    if (!node) {
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            cbRef.current();
          }
        }
      },
      {
        rootMargin,
        threshold,
      },
    );

    observer.observe(node);

    return () => observer.disconnect();
  }, [enabled, rootMargin, threshold]);

  return (node) => {
    nodeRef.current = node;
  };
}
