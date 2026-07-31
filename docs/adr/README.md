# Architecture Decision Records

We record every non-obvious architectural choice as an ADR — a short
markdown file that captures the context, the decision, and its
consequences at the moment it was made.

## Rules

- One ADR per decision, one decision per ADR.
- File name: `NNNN-kebab-case-title.md`, four-digit zero-padded number.
- Never rewrite history — supersede instead.
- Statuses: `Proposed` | `Accepted` | `Superseded by NNNN` | `Deprecated`.

## Template

```
# NNNN. Title

- Status: Accepted
- Date: YYYY-MM-DD

## Context
Why is this decision being made? What forces are at play?

## Decision
The choice we made, stated clearly.

## Consequences
What follows — good, bad, and neutral. What becomes easier?
What becomes harder?

## Alternatives considered
Other options and why we didn't pick them.
```