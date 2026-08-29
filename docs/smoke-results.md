# Smoke test — Day 50c

Date: 2026-08-29
Duration: N/A
Branch: test/smoke-day50

## Results

| Section | Status | Notes |
|---------|--------|-------|
| A. Pre-flight | ⚠️ | Not fully re-verified in the captured run |
| B. Infrastructure health | ✅ | `/alive` and `/health` returned `Healthy` |
| C. Auth flow | ✅ | Login succeeded and JWT authentication worked |
| D. Catalog | ✅ | Products endpoint returned products; 8 products currently exist |
| E. Cart + Redis | ✅ | Add/get/delete cart operations worked; inventory snapshot verified in Redis |
| F. Checkout + Order pipeline | ✅ | Checkout created orders; OrderPlaced and PaymentCaptured outbox events were processed |
| G. Reviews | ✅ | Review creation, approval, rejection and public filtering verified |
| H. Background workers | ✅ | Inventory snapshot, abandoned-cart reminder and dead-letter monitoring verified |
| I. Deep inspection | ✅ | SQL counts and outbox processing checks passed |
 
## Issues found

- Stripe webhook signature verification correctly rejects invalid signatures.
- Stripe CLI webhook forwarding with the configured signing secret was verified successfully.
- `poison-ping_error` contains 2 existing messages. `DeadLetterMonitorJob` correctly detects and logs the non-empty error queue.
- Redis `redis-cli` access from the local/container shell was resetting, but Redis Commander successfully accessed the Redis instance and verified `inventory:snapshot`.
- The smoke-test abandoned-cart interval/threshold was temporarily shortened for testing and must remain restored to the normal 1-hour / 24-hour configuration.

## Follow-ups filed

- Consider investigating the existing `poison-ping_error` messages separately if they are not intentional test fixtures.
- Keep the reusable smoke script updated as additional API/workflow tests are added.
