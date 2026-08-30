import type { ProblemDetails } from './problem-details';

export class ApiError extends Error {
  public readonly status: number;
  public readonly problem: ProblemDetails | undefined;

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  get isUnauthorized(): boolean {
    return this.status === 401;
  }

  get isForbidden(): boolean {
    return this.status === 403;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }

  get isValidation(): boolean {
    return this.status === 400 || this.status === 422;
  }

  /**
   * Collect field errors from ProblemDetails.errors into a flat map.
   * Empty object when there are none.
   */
  get fieldErrors(): Record<string, string> {
    const errors = this.problem?.errors;
    if (!errors) return {};

    const out: Record<string, string> = {};

    for (const key of Object.keys(errors)) {
      const list = errors[key];

      if (list && list.length > 0 && typeof list[0] === 'string') {
        out[key] = list[0];
      }
    }

    return out;
  }
}
