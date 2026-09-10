export interface TelemetryContext {
  route?: string;
  userId?: string;
  extra?: Record<string, unknown>;
}

/**
 * Send a caught error to whatever telemetry backend we adopt.
 * Today: console.error with a distinctive tag. Phase 6 wires Sentry.
 */
export function logToTelemetry(error: unknown, ctx: TelemetryContext = {}): void {
  const payload = {
    tag: '[telemetry]',
    when: new Date().toISOString(),
    ...ctx,
    error: normalizeError(error),
  };

  // Route to console in dev, and to an endpoint in prod (Phase 6).
  if (import.meta.env.DEV) {
    console.error(payload.tag, payload);
    return;
  }

  // Placeholder for prod delivery:
  // navigator.sendBeacon('/api/v1/telemetry', JSON.stringify(payload));
  console.error(payload.tag, payload);
}

function normalizeError(err: unknown): { name: string; message: string; stack?: string } {
  if (err instanceof Error) {
    const result: { name: string; message: string; stack?: string } = {
      name: err.name,
      message: err.message,
    };

    if (err.stack) {
      result.stack = err.stack;
    }

    return result;
  }

  return { name: 'NonError', message: String(err) };
}
