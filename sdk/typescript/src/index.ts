export type KycHeadersInput = {
  bearerToken?: string;
  extra?: Record<string, string>;
};

/** Build HTTP headers for KYC Web API calls. */
export function headers(input: KycHeadersInput = {}): Record<string, string> {
  const h: Record<string, string> = {
    "Content-Type": "application/json",
    Accept: "application/json",
  };
  if (input.bearerToken) h.Authorization = `Bearer ${input.bearerToken}`;
  if (input.extra) Object.assign(h, input.extra);
  return h;
}

export type FetchInitInput = KycHeadersInput & {
  baseUrl: string;
};

/** Absolute health endpoint used by Compose / probes. */
export function healthUrl(baseUrl: string): string {
  return baseUrl.replace(/\/$/, "") + "/health";
}

/** Build a RequestInit-friendly headers map + normalized base URL. */
export function clientOptions(input: FetchInitInput) {
  return {
    baseUrl: input.baseUrl.replace(/\/$/, ""),
    headers: headers(input),
  };
}
