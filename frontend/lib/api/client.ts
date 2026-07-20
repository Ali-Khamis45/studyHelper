import { useAuthStore, type AuthUser } from "@/lib/stores/authStore";

// Routed through the /api/backend rewrite (see next.config.ts) rather than the backend origin
// directly, so the refresh-token cookie the backend sets is first-party to the browser and
// actually gets stored/sent (a cross-origin Set-Cookie here would be silently dropped/blocked).
// Client-side use only — there's no implicit origin for a relative fetch() on the server.
export const API_BASE_URL = "/api/backend";

export class ApiError extends Error {
  constructor(
    public status: number,
    public body: unknown,
  ) {
    super(`API request failed with status ${status}`);
  }
}

let refreshPromise: Promise<boolean> | null = null;

async function refreshAccessToken(): Promise<boolean> {
  refreshPromise ??= (async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/v1/auth/refresh`, {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) return false;

      const data = (await res.json()) as { accessToken: string; user: AuthUser };
      useAuthStore.getState().setSession(data.user, data.accessToken);
      return true;
    } catch {
      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

interface ApiFetchOptions extends Omit<RequestInit, "body"> {
  body?: unknown;
  skipAuthRetry?: boolean;
}

export async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  const { body, skipAuthRetry, headers, ...rest } = options;
  const accessToken = useAuthStore.getState().accessToken;

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...rest,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (res.status === 401 && !skipAuthRetry) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      return apiFetch<T>(path, { ...options, skipAuthRetry: true });
    }
    useAuthStore.getState().clearSession();
  }

  if (!res.ok) {
    const errorBody = await res.json().catch(() => null);
    throw new ApiError(res.status, errorBody);
  }

  if (res.status === 204) return undefined as T;

  return (await res.json()) as T;
}
