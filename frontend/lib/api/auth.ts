import { apiFetch } from "@/lib/api/client";
import type { AuthUser } from "@/lib/stores/authStore";

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthUser;
}

export function register(input: { email: string; password: string; displayName: string }) {
  return apiFetch<AuthResponse>("/api/v1/auth/register", { method: "POST", body: input });
}

export function login(input: { email: string; password: string }) {
  return apiFetch<AuthResponse>("/api/v1/auth/login", { method: "POST", body: input });
}

export function refresh() {
  return apiFetch<AuthResponse>("/api/v1/auth/refresh", { method: "POST", skipAuthRetry: true });
}

export function logout() {
  return apiFetch<void>("/api/v1/auth/logout", { method: "POST" });
}

export function getMe() {
  return apiFetch<AuthUser>("/api/v1/auth/me");
}
