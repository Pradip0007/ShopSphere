import { useState } from 'react';
import { Dialog, DialogContent, DialogTitle, DialogTrigger } from '@/shared/ui/Dialog';

interface ProductGalleryProps {
  images: string[];
  productName: string;
}

export function ProductGallery({ images, productName }: ProductGalleryProps): React.JSX.Element {
  const [activeIndex, setActiveIndex] = useState(0);
  const activeImage = images[activeIndex] ?? images[0] ?? null;

  return (
    <div className="grid gap-3">
      <Dialog>
        <DialogTrigger asChild>
          <button
            type="button"
            className="aspect-square overflow-hidden rounded-lg border border-[var(--color-border)] bg-[var(--color-surface-muted)]"
            aria-label={`Zoom image of ${productName}`}
          >
            {activeImage ? (
              <img src={activeImage} alt={productName} className="h-full w-full object-cover" />
            ) : (
              <span className="grid h-full place-items-center text-[var(--color-text-muted)]">
                No image available
              </span>
            )}
          </button>
        </DialogTrigger>

        <DialogContent className="w-full max-w-3xl border-none bg-transparent p-0 shadow-none">
          <DialogTitle className="sr-only">{productName} full-size image</DialogTitle>
          {activeImage ? (
            <img src={activeImage} alt={productName} className="h-auto w-full rounded-lg" />
          ) : (
            <div className="rounded-lg bg-[var(--color-surface)] p-12 text-center text-[var(--color-text-muted)]">
              No image available
            </div>
          )}
        </DialogContent>
      </Dialog>

      {images.length > 1 && (
        <ul
          className="grid gap-2"
          style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(60px, 1fr))' }}
        >
          {images.map((src, index) => {
            const isActive = index === activeIndex;

            return (
              <li key={src}>
                <button
                  type="button"
                  onClick={() => setActiveIndex(index)}
                  aria-label={`Show image ${index + 1}`}
                  aria-current={isActive ? 'true' : undefined}
                  className={`aspect-square overflow-hidden rounded-md border-2 ${
                    isActive ? 'border-brand-500' : 'border-[var(--color-border)]'
                  }`}
                >
                  <img src={src} alt="" className="h-full w-full object-cover" />
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
