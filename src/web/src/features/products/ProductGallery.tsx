import { useState } from 'react';
import { Dialog, DialogContent, DialogTitle, DialogTrigger } from '@/shared/ui/Dialog';

interface ProductGalleryProps {
  images: string[];
  productName: string;
  category: string;
}

const PREVIEW_VARIANTS = [
  'from-sky-100 via-blue-50 to-indigo-100',
  'from-violet-100 via-fuchsia-50 to-rose-100',
  'from-amber-100 via-orange-50 to-rose-100',
];

export function ProductGallery({
  images,
  productName,
  category,
}: ProductGalleryProps): React.JSX.Element {
  const [activeIndex, setActiveIndex] = useState(0);
  const activeImage = images[activeIndex] ?? images[0] ?? null;
  const previewCount = images.length > 0 ? images.length : PREVIEW_VARIANTS.length;

  return (
    <div className="grid content-start gap-4">
      <Dialog>
        <DialogTrigger asChild>
          <button
            type="button"
            className="relative aspect-square overflow-hidden rounded-2xl border border-slate-200 bg-white text-left shadow-sm focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600"
            aria-label={`Zoom image of ${productName}`}
          >
            {activeImage ? (
              <img
                src={activeImage}
                alt={productName}
                className="h-full w-full object-contain p-8"
              />
            ) : (
              <ProductArtwork category={category} index={activeIndex} />
            )}
            <span className="absolute left-4 top-4 rounded-full border border-white/70 bg-white/85 px-3 py-1 text-xs font-semibold text-slate-700 shadow-sm backdrop-blur">
              {activeImage ? 'Product image' : 'Sample product preview'}
            </span>
            <span className="absolute bottom-4 right-4 rounded-full bg-white/85 px-3 py-1.5 text-xs font-medium text-slate-600 shadow-sm backdrop-blur">
              Click to enlarge
            </span>
          </button>
        </DialogTrigger>

        <DialogContent className="w-full max-w-3xl overflow-hidden border-none bg-white p-3 shadow-xl">
          <DialogTitle className="sr-only">{productName} full-size image</DialogTitle>
          {activeImage ? (
            <img
              src={activeImage}
              alt={productName}
              className="h-auto max-h-[80vh] w-full object-contain"
            />
          ) : (
            <div className="aspect-square max-h-[80vh]">
              <ProductArtwork category={category} index={activeIndex} />
            </div>
          )}
        </DialogContent>
      </Dialog>

      <div className="flex items-center justify-between gap-3">
        <p className="text-xs text-slate-500">
          {activeImage
            ? `${images.length} product views`
            : 'Illustrative preview · photos coming soon'}
        </p>
        <ul aria-label="Product views" className="m-0 flex list-none gap-2 p-0">
          {Array.from({ length: previewCount }, (_, index) => {
            const isActive = index === activeIndex;

            return (
              <li key={images[index] ?? `preview-${index}`}>
                <button
                  type="button"
                  onClick={() => setActiveIndex(index)}
                  aria-label={`Show image ${index + 1}`}
                  aria-current={isActive ? 'true' : undefined}
                  className={`grid size-12 place-items-center overflow-hidden rounded-lg border-2 transition ${
                    isActive
                      ? 'border-blue-600 ring-2 ring-blue-100'
                      : 'border-slate-200 hover:border-slate-400'
                  }`}
                >
                  {images[index] ? (
                    <img src={images[index]} alt="" className="h-full w-full object-cover" />
                  ) : (
                    <span
                      aria-hidden="true"
                      className={`grid h-full w-full place-items-center bg-gradient-to-br ${PREVIEW_VARIANTS[index]}`}
                    >
                      <ProductIcon className="size-5 text-slate-600" />
                    </span>
                  )}
                </button>
              </li>
            );
          })}
        </ul>
      </div>
    </div>
  );
}

function ProductArtwork({
  category,
  index,
}: {
  category: string;
  index: number;
}): React.JSX.Element {
  return (
    <div
      className={`relative grid h-full w-full place-items-center overflow-hidden bg-gradient-to-br ${PREVIEW_VARIANTS[index % PREVIEW_VARIANTS.length]}`}
    >
      <div
        aria-hidden="true"
        className="absolute -right-12 -top-12 size-56 rounded-full border-[28px] border-white/25"
      />
      <div
        aria-hidden="true"
        className="absolute -bottom-20 -left-10 size-64 rounded-full border-[36px] border-white/20"
      />
      <div className="relative grid size-48 place-items-center rounded-[2rem] border border-white/70 bg-white/55 text-slate-700 shadow-xl shadow-slate-900/10 backdrop-blur-sm sm:size-60">
        <ProductIcon className="size-24 drop-shadow-sm sm:size-32" />
        <span className="absolute bottom-5 rounded-full bg-white/75 px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] text-slate-600">
          {category}
        </span>
      </div>
      <p className="absolute bottom-5 left-5 text-xs font-medium text-slate-600/80">
        Product photo coming soon
      </p>
    </div>
  );
}

function ProductIcon({ className }: { className: string }): React.JSX.Element {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 96 96"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
    >
      <path d="m48 11 34 18v38L48 85 14 67V29L48 11Z" fill="white" fillOpacity=".72" />
      <path d="m14 29 34 19 34-19M48 48v37" />
      <path d="m30 20 35 19" strokeOpacity=".45" />
      <path d="M39 53v14l18 10V63L39 53Z" fill="white" fillOpacity=".55" />
    </svg>
  );
}
