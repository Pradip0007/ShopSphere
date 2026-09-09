# ADR 0003: Defer React Server Components

- **Status:** Accepted
- **Date:** {{ today's date }}
- **Deciders:** Solo dev
- **Related:** ADR 0001 (SPA + separate API), ADR 0002 (Vite as build tool)

## Context

Phase 4 built the ShopSphere frontend as a Vite + React 19 SPA. React Server
Components (RSC) offer:

- Zero-JS data-fetching components (bundle size wins).
- Native SEO with real HTML from the server.
- Co-located data + UI (no `useQuery` ceremony for read paths).

Frameworks that ship first-class RSC today: Next.js (App Router), Waku,
Remix (v3 preview). None run on plain Vite + our current setup.

Adopting RSC now would require:

- Replacing Vite with Next.js (or Waku on a fork of Vite RSC support).
- Rewriting TanStack Router → Next App Router or Waku router.
- Introducing a Node server tier (breaks our pure-static SPA deploy).
- Changing the auth model — JWT in Redux stops making sense on the server tier.

## Options considered

### A) Adopt Next.js App Router now

**Pros:** Best RSC support, wide community, excellent Vercel deploy path.
**Cons:** Full rewrite of Days 51–66; Next opinions conflict with our existing
router + Redux + Query stack; Vercel deploy vs. our .NET-friendly Static Web
Apps target adds friction.

### B) Adopt Waku on Vite

**Pros:** Preserves Vite, small footprint, community-driven, easy to try.
**Cons:** Immature — 0.x, few production references, no company backing
migration matters if it fizzles. Router is minimal (no equivalent to TanStack
Router's search-param validation).

### C) Stay pure SPA, revisit in Phase 7 (default)

**Pros:** Zero disruption. Our stack is a real, well-tested combination. When
RSC matures on Vite (React team's `react-server-dom-vite` is in progress), we
can adopt without changing frameworks.
**Cons:** We keep paying the "extra API round trip" cost for product listings
until then. Also carry the SEO cost — but we don't sell SEO to consumers today
(this is a portfolio + admin app).

## Decision

**Choose Option C.** Defer RSC. Keep the pure Vite SPA + TanStack Query + Redux
stack we shipped in Days 51-66.

Revisit when any of these are true:

- `react-server-dom-vite` reaches a stable release.
- We hit a real SEO-driven revenue gap (this needs analytics, not intuition).
- We need to serve a fully public-facing storefront (currently: auth-gated).

## Evidence (playground measurements)

Isolated `src/rsc-playground/` (Waku minimal) vs `src/web/` (Vite production
build), same 10-item product list:

| Metric                | Vite SPA | Waku RSC | Delta |
| --------------------- | -------- | -------- | ----- |
| First-load JS (gzip)  | 148 KB   | 42 KB    | -72%  |
| Time to interactive   | 810 ms   | 320 ms   | -60%  |
| Lighthouse Perf       | 91       | 98       | +7    |
| Lines of code changed | 0        | ~2,400   | full rewrite |

(Numbers illustrative — plug in your actual measurements.)

**Interpretation:** RSC would win on Perf and bundle size, but the code delta
represents rewriting every route, every provider, every mutation. That trade
is not worth it in Phase 4.

## Consequences

- **Positive:** Ship Phase 4 on schedule. Existing tests and patterns keep
  working. The auth model, Redux slice, and TanStack Query cache all remain
  valid.
- **Negative:** Client bundle stays around 150 KB gzip. SEO for public product
  pages will require a separate strategy (SSR via Vite's `renderToString`,
  or a pre-rendered marketing site).
- **Follow-up:** Add a lightweight prerender step for the marketing homepage
  in Phase 7 if organic traffic becomes a goal.

## Ergonomic frictions observed

Recording specifics so we don't re-litigate:

1. **Passing callbacks from server to client requires a Server Action or a
   client-provided handler.** Every "onClick that mutates" needs redesign.
2. **State libraries (Redux, Zustand) are client-only.** Auth state stays
   client, so half the app remains "client" anyway.
3. **Component decoration (Storybook, testing) fragments — server vs client
   components have different test setups.**
4. **RSC + streaming + suspense + Query is not yet a settled pattern.**
   Multiple RFCs still in flight.

## Playground disposition

- Retain `src/rsc-playground/` for one more sprint as a reference.
- Delete after Phase 5 unless someone touches it. Add to `.gitignore` if
  the deps become noisy.