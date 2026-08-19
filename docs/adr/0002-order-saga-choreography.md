# ADR-002: Order Saga — Choreography over Orchestration

- Status: Accepted
- Date: 2026-08-19
- Author: Solo (initials or handle)
- Supersedes: —
- Superseded by: —

## Context

An order in ShopSphere goes through several coordinated steps after the customer
clicks "Place order":

1. Reserve inventory for each line.
2. Authorize payment against the recorded card / payment method.
3. Confirm the order (persist final state, dispatch fulfilment).
4. Send a confirmation email.

Each step involves a different bounded context (Inventory, Payments, Ordering,
Notifications). Any of them can fail, and failures downstream require
undoing (or compensating) the work of earlier steps.

We need to decide how these steps are coordinated.