import { Link } from '@tanstack/react-router';

interface BrandLogoProps {
  className?: string;
  light?: boolean;
}

export function BrandLogo({ className, light = false }: BrandLogoProps): React.JSX.Element {
  return (
    <Link
      to="/"
      aria-label="ShopSphere home"
      className={`inline-flex items-center gap-2 rounded-sm focus-visible:outline-brand-500 ${className ?? ''}`}
    >
      {light ? (
        <img src="/shopsphere-logo.png" alt="" className="h-16 w-auto max-w-full object-contain" />
      ) : (
        <>
          <svg
            aria-hidden="true"
            viewBox="0 0 48 48"
            className="h-10 w-10"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <circle cx="24" cy="24" r="21" fill="#087F8C" />
            <path
              d="M11 28.5c2.4 7.3 10.3 11.2 17.6 8.8 5.8-1.9 9.3-7.3 9-13.1"
              stroke="#F59E0B"
              strokeWidth="3.5"
              strokeLinecap="round"
            />
            <path
              d="M15.2 24.5h3l2.8 10h11.4l3-7.6H20"
              stroke="white"
              strokeWidth="2.4"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            <circle cx="23" cy="38" r="1.8" fill="white" />
            <circle cx="31" cy="38" r="1.8" fill="white" />
          </svg>
          <span className="text-2xl font-bold tracking-tight">
            Shop<span className="text-amber-400">S</span>phere
          </span>
        </>
      )}
    </Link>
  );
}
