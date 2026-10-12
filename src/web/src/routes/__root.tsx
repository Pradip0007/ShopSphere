import { type QueryClient, QueryErrorResetBoundary, useQueryClient } from '@tanstack/react-query';
import { createRootRouteWithContext, Link, Outlet, useRouterState } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import { ErrorBoundary } from 'react-error-boundary';
import { AppError } from '@/app/AppError';
import { logoutApi } from '@/features/auth/api';
import { LogoutButton } from '@/features/auth/LogoutButton';
import { CartBadge } from '@/features/cart/CartBadge';
import { NewsletterForm } from '@/features/newsletter/NewsletterForm';
import { router } from '@/router';
import { HubStatusPill } from '@/shared/lib/signalr/HubStatusPill';
import { logToTelemetry } from '@/shared/lib/telemetry';
import { BrandLogo } from '@/shared/ui/BrandLogo';
import { logout, selectAuth, selectHasRole } from '@/store/auth.slice';
import { useAppDispatch, useAppSelector } from '@/store/hooks';

export interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
  notFoundComponent: () => (
    <section className="grid max-w-md mx-auto gap-3 py-12 text-center">
      <h1 className="text-2xl font-semibold">404 — Page not found</h1>
      <p className="text-[var(--color-text-muted)]">The page you're looking for isn't here.</p>
      <a href="/" className="text-brand-600 underline">
        Return home
      </a>
    </section>
  ),
});

function RootLayout(): React.JSX.Element {
  const { user } = useAppSelector(selectAuth);
  const isAdmin = useAppSelector(selectHasRole('admin'));
  const pathname = useRouterState({ select: (state) => state.location.pathname });
  const isAuthPage = pathname === '/login' || pathname === '/register';
  const dispatch = useAppDispatch();
  const queryClient = useQueryClient();

  async function handleLogout(): Promise<void> {
    try {
      await logoutApi();
    } finally {
      dispatch(logout());
      queryClient.clear();

      await router.navigate({
        to: '/login',
        search: {},
      });
    }
  }

  return (
    <div className="flex h-dvh flex-col overflow-hidden">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:m-2 focus:rounded-md focus:bg-white focus:px-4 focus:py-2 focus:text-black"
      >
        Skip to main content
      </a>
      {isAuthPage ? (
        <header className="flex justify-center border-b border-slate-200 px-4 py-3">
          <BrandLogo light />
        </header>
      ) : (
        <header className="text-white">
          <div className="flex min-h-[76px] flex-wrap items-center gap-x-4 gap-y-3 bg-[#131921] px-4 py-3 lg:flex-nowrap lg:px-6">
            <BrandLogo className="shrink-0 focus-visible:outline-white" />

            <div
              className="hidden shrink-0 items-center gap-2 rounded-sm px-2 py-1 md:flex"
              title="Delivery location is a display placeholder"
            >
              <svg
                aria-hidden="true"
                viewBox="0 0 24 24"
                className="h-5 w-5"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.8"
              >
                <path d="M19 10c0 5-7 11-7 11S5 15 5 10a7 7 0 1 1 14 0Z" />
                <circle cx="12" cy="10" r="2.3" />
              </svg>
              <span className="grid leading-tight">
                <span className="text-xs text-slate-300">Deliver to</span>
                <span className="font-semibold">India</span>
              </span>
            </div>

            <search className="order-last w-full lg:order-none lg:min-w-[240px] lg:flex-1">
              <form
                action="/products"
                method="get"
                className="flex h-11 w-full overflow-hidden rounded-md bg-white text-slate-900 focus-within:outline focus-within:outline-2 focus-within:outline-amber-400"
              >
                <label htmlFor="header-search-scope" className="sr-only">
                  Search category
                </label>
                <select
                  id="header-search-scope"
                  defaultValue="all"
                  aria-label="Search category"
                  className="hidden max-w-24 border-r border-slate-300 bg-slate-100 px-3 text-sm text-slate-700 sm:block"
                >
                  <option value="all">All</option>
                </select>
                <label htmlFor="header-search" className="sr-only">
                  Search ShopSphere
                </label>
                <input
                  id="header-search"
                  type="search"
                  name="q"
                  placeholder="Search ShopSphere"
                  className="min-w-0 flex-1 px-3 text-base outline-none"
                />
                <button
                  type="submit"
                  aria-label="Search"
                  className="grid w-14 shrink-0 place-items-center bg-amber-400 text-slate-900 transition-colors hover:bg-amber-300"
                >
                  <svg
                    aria-hidden="true"
                    viewBox="0 0 24 24"
                    className="h-6 w-6"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <circle cx="10.8" cy="10.8" r="6.8" />
                    <path d="m16 16 5 5" strokeLinecap="round" />
                  </svg>
                </button>
              </form>
            </search>

            <span
              role="img"
              className="hidden shrink-0 rounded-sm px-2 py-1 text-sm font-semibold sm:inline"
              aria-label="Language: English"
              title="Language selector is a display placeholder"
            >
              ◎ EN
            </span>

            <div className="flex shrink-0 items-center gap-4">
              {user ? (
                <>
                  <HubStatusPill />
                  <Link
                    to="/orders"
                    data-testid="user-greeting"
                    className="grid max-w-44 leading-tight hover:outline hover:outline-1 hover:outline-white"
                  >
                    <span className="truncate text-xs">Hello, {user.email}</span>
                    <span className="font-bold">Account &amp; Lists</span>
                  </Link>
                  <form action={handleLogout}>
                    <LogoutButton />
                  </form>
                </>
              ) : (
                <>
                  <Link
                    to="/login"
                    data-testid="nav-login"
                    search={{}}
                    className="grid leading-tight hover:outline hover:outline-1 hover:outline-white"
                  >
                    <span className="text-xs">Hello, sign in</span>
                    <span className="font-bold">Account &amp; Lists</span>
                  </Link>
                  <Link
                    to="/register"
                    data-testid="nav-register"
                    className="text-sm font-semibold hover:underline"
                  >
                    Register
                  </Link>
                </>
              )}

              {user && (
                <Link
                  to="/orders"
                  data-testid="nav-orders"
                  className="hidden leading-tight hover:outline hover:outline-1 hover:outline-white xl:grid"
                >
                  <span className="text-xs">Returns</span>
                  <span className="font-bold">&amp; Orders</span>
                </Link>
              )}

              <Link
                to="/cart"
                data-testid="cart-page-link"
                aria-label="Open shopping cart"
                className="flex items-end gap-1 rounded-sm px-1 py-1 font-bold hover:outline hover:outline-1 hover:outline-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
              >
                <svg
                  aria-hidden="true"
                  viewBox="0 0 32 28"
                  className="h-8 w-9"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M2 3h4l3.2 15.2a2 2 0 0 0 2 1.6h13.4a2 2 0 0 0 1.9-1.4L30 8H8" />
                  <circle cx="12" cy="25" r="1.5" />
                  <circle cx="24" cy="25" r="1.5" />
                </svg>
                <CartBadge />
              </Link>
            </div>
          </div>

          <nav
            aria-label="Primary"
            data-testid="primary-nav"
            className="flex min-h-12 items-center gap-6 overflow-x-auto bg-[#232f3e] px-4 text-sm font-medium sm:px-6"
          >
            <Link
              to="/products"
              data-testid="nav-all-products"
              className="shrink-0 font-bold hover:outline hover:outline-1 hover:outline-white"
            >
              ☰ All
            </Link>
            <Link to="/" data-testid="nav-home" className="shrink-0 hover:underline">
              Home
            </Link>
            <Link to="/products" data-testid="nav-products" className="shrink-0 hover:underline">
              Products
            </Link>
            <span className="shrink-0 text-slate-200" title="Coming soon">
              Today&apos;s Deals
            </span>
            <span className="shrink-0 text-slate-200" title="Coming soon">
              New Releases
            </span>
            <span className="shrink-0 text-slate-200" title="Coming soon">
              Customer Service
            </span>
            {user && (
              <Link
                to="/orders"
                data-testid="nav-orders-secondary"
                className="shrink-0 hover:underline xl:hidden"
              >
                Orders
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin" data-testid="nav-admin" className="shrink-0 hover:underline">
                Admin
              </Link>
            )}
          </nav>
        </header>
      )}
      <div className="flex min-h-0 flex-1 flex-col overflow-y-auto">
        <main
          id="main-content"
          data-testid="main-content"
          className={`flex-1 ${isAuthPage ? 'px-4 py-8' : 'p-8'}`}
        >
          <QueryErrorResetBoundary>
            {({ reset }) => (
              <ErrorBoundary
                FallbackComponent={AppError}
                onError={(error, info) =>
                  logToTelemetry(error, {
                    route: window.location.pathname,
                    extra: { componentStack: info.componentStack },
                  })
                }
                onReset={reset}
              >
                <Outlet />
              </ErrorBoundary>
            )}
          </QueryErrorResetBoundary>
        </main>
        <footer role="contentinfo" className="mt-auto bg-[#0b1118] text-slate-200">
          <a
            href="#main-content"
            className="flex min-h-11 items-center justify-center bg-[#263547] px-4 text-sm hover:bg-[#31445a]"
          >
            Back to top
          </a>

          <div className="bg-[#1e2b3c] px-4 py-7 sm:px-8">
            <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-center gap-4">
              <BrandLogo />
              <div className="flex flex-wrap items-center justify-center gap-3">
                <fieldset
                  title="Currency selection is a display placeholder"
                  className="rounded border border-slate-500 px-4 py-2 text-sm"
                >
                  <legend className="sr-only">Display currency</legend>₹ &nbsp; INR - Indian Rupee
                </fieldset>
                <fieldset
                  title="Region selection is a display placeholder"
                  className="rounded border border-slate-500 px-4 py-2 text-sm"
                >
                  <legend className="sr-only">Display region</legend>
                  🇮🇳 &nbsp; India
                </fieldset>
              </div>
            </div>
          </div>

          <div className="mx-auto grid max-w-6xl grid-cols-2 gap-x-8 gap-y-10 px-6 py-10 sm:grid-cols-3 lg:grid-cols-5 lg:px-8">
            <nav aria-label="Shop" className="grid content-start gap-3 text-sm">
              <h2 className="font-bold text-white">Shop</h2>
              <Link to="/products" className="text-slate-400 hover:text-white hover:underline">
                Browse products
              </Link>
              <Link to="/cart" className="text-slate-400 hover:text-white hover:underline">
                Your cart
              </Link>
              <span className="text-slate-400" title="Coming soon">
                Categories
              </span>
              <span className="text-slate-400" title="Coming soon">
                Featured collections
              </span>
            </nav>

            <nav aria-label="Your account" className="grid content-start gap-3 text-sm">
              <h2 className="font-bold text-white">Your account</h2>
              {user ? (
                <Link to="/orders" className="text-slate-400 hover:text-white hover:underline">
                  Your orders
                </Link>
              ) : (
                <Link
                  to="/login"
                  search={{}}
                  className="text-slate-400 hover:text-white hover:underline"
                >
                  Sign in
                </Link>
              )}
              <Link to="/register" className="text-slate-400 hover:text-white hover:underline">
                Create an account
              </Link>
              <span className="text-slate-400" title="Coming soon">
                Saved items
              </span>
            </nav>

            <nav aria-label="About ShopSphere" className="grid content-start gap-3 text-sm">
              <h2 className="font-bold text-white">About ShopSphere</h2>
              <span className="text-slate-400" title="Coming soon">
                About us
              </span>
              <span className="text-slate-400" title="Coming soon">
                Careers
              </span>
              <span className="text-slate-400" title="Coming soon">
                Newsroom
              </span>
              <span className="text-slate-400" title="Coming soon">
                Sell with us
              </span>
            </nav>

            <nav aria-label="Help and policies" className="grid content-start gap-3 text-sm">
              <h2 className="font-bold text-white">Help &amp; policies</h2>
              <a
                href="mailto:support@shopsphere.local"
                className="text-slate-400 hover:text-white hover:underline"
              >
                Contact support
              </a>
              <span className="text-slate-400" title="Policy page coming soon">
                Conditions of Use
              </span>
              <span className="text-slate-400" title="Policy page coming soon">
                Privacy Notice
              </span>
              <span className="text-slate-400" title="Policy page coming soon">
                Cookies Notice
              </span>
            </nav>

            <div className="col-span-2 grid content-start gap-3 rounded-md bg-white p-4 text-slate-900 sm:col-span-1">
              <NewsletterForm />
            </div>
          </div>

          <div className="border-t border-slate-700 px-4 py-6 text-center text-sm">
            <nav
              aria-label="Legal information"
              className="flex flex-wrap justify-center gap-x-5 gap-y-2 text-slate-300"
            >
              <span>Conditions of Use</span>
              <span>Privacy Notice</span>
              <span>Cookies Notice</span>
              <span>Interest-Based Ads Notice</span>
            </nav>
            <p className="mt-4 text-slate-400">
              © {new Date().getFullYear()} ShopSphere. Your global marketplace.
            </p>
          </div>
        </footer>
      </div>
      {import.meta.env.DEV && (
        <TanStackRouterDevtools position="bottom-right" containerElement="div" />
      )}
    </div>
  );
}
