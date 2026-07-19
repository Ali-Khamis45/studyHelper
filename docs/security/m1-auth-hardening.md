# M1 Authentication — Security Hardening Audit

Date: 2026-07-20 · Scope: the existing M1 password-auth surface only (register, login, refresh,
logout, me). No new endpoints or features were added — every change below either hardens an
existing code path or documents why an existing choice is already correct. Findings verified
against the actual source, not from memory.

Verdict key: **✅ already compliant** (no change) · **🔧 hardened** (changed in this pass) ·
**⚠️ accepted risk** (documented, deliberately deferred).

## Findings

| # | Item | Verdict | Detail |
|---|---|---|---|
| 1 | JWT algorithm & key length | ✅ | HS256. The configured signing key is a 64-byte (512-bit) random value, but `JwtTokenService` feeds it to HMAC as its UTF8-encoded **base64 string**, not the decoded raw bytes — the effective key material is ~88 bytes (~528 bits of entropy). Both readings clear RFC 7518's 256-bit HS256 floor with a wide margin. Not a weakness; documented here so "512-bit" isn't assumed to be the literal key-material size later. |
| 2 | Access tokens contain only minimum claims | 🔧 | Was: `sub`, `email`, `name`, `jti`. `CurrentUserService` (the only claim consumer) reads `sub` exclusively, and `GetMeQueryHandler` re-fetches the user from the DB rather than trusting token claims — `email`/`name` were unused PII sitting in a token anyone can base64-decode. **Now**: `sub` + `jti` only (`Infrastructure/Identity/JwtTokenService.cs`). |
| 3 | Refresh tokens ≥256 bits entropy | ✅ | `RefreshTokenGenerator.GenerateRawToken()` — `RandomNumberGenerator.GetBytes(32)`, exactly 256 bits, CSPRNG-sourced. |
| 4 | Refresh token hash ≥SHA-256 | ✅ | `RefreshTokenGenerator.Hash()` — `SHA256.HashData`. |
| 5 | Constant-time comparisons | ✅ | Password verification goes through `IPasswordHasher<User>.VerifyHashedPassword`, which uses `CryptographicOperations.FixedTimeEquals` internally. JWT signature validation is handled by `Microsoft.IdentityModel.Tokens` (constant-time HMAC verify). Refresh-token matching never compares the raw secret in application code at all — it hashes the presented token first, then does an indexed database equality lookup on the digest. That sidesteps the raw-secret timing-attack scenario constant-time comparison exists to prevent; there was no missing comparison to add. |
| 6 | Cookie flags appropriate for dev vs prod | ✅ | `AuthEndpoints.SetRefreshCookie`: `HttpOnly=true`; `Secure = http.Request.IsHttps` (dynamically `false` on local http, `true` behind real TLS — no environment branching needed); `SameSite=Strict`; `Path=/`. Already environment-adaptive. |
| 7 | CORS least-privilege | ✅ | No `AddCors`/`UseCors` is registered anywhere (`Program.cs`, verified by reading, not assumed). Correct by construction: the frontend only ever reaches this API through the same-origin Next.js rewrite (`frontend/next.config.ts`'s `/api/backend/*` proxy), so no browser cross-origin request needs to be allowed. Default-deny is the least-privilege posture; a comment was added in `Program.cs` so a future milestone doesn't "fix" this by opening a wildcard policy. If a genuinely cross-origin client (mobile app, separate frontend origin) is ever added, add a narrowly-scoped policy naming exact allowed origins then. |
| 8 | Lockout / brute-force protection | 🔧 | Was: no attempt tracking at all — unlimited password guesses against any account. **Now**, two layers: (a) per-account lockout — `User.FailedLoginAttempts` / `User.LockedOutUntilUtc` (`Domain/Identity/User.cs`), `RegisterFailedLogin`/`RegisterSuccessfulLogin`/`IsLockedOut`, enforced in `LoginUserCommandHandler` before password verification, default 5 attempts / 15-minute lockout, configurable via `Lockout:*`; (b) IP-based rate limiting on `/register`, `/login`, `/refresh` via ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting` (fixed window, default 20 req/min/IP, configurable via `RateLimiting:*`), returning `429`. Account lockout stops targeted brute force against one account; IP rate limiting stops broad/distributed guessing. **Confirmed with the user**: a locked-out login returns the identical generic 401 as a wrong password — lockout state is never distinguishable in the response, so it can't be used as an account-existence/near-lockout oracle. |
| 9 | Password policy configurable | 🔧 | Was: `MinimumLength(8)`/`MaximumLength(128)` hardcoded in `RegisterUserCommandValidator`. **Now**: `PasswordPolicyOptions` (same defaults) bound from config, validator takes `IOptions<PasswordPolicyOptions>` via constructor injection. |
| 10 | Security headers | 🔧 | Was: none. **Now**: `Api/Middleware/SecurityHeadersMiddleware.cs` (`X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`) applied to every response, plus `app.UseHsts()` gated to non-Development (HSTS is meaningless — and wrong to send — over local http). |
| 11 | Auth events logged, without leaking secrets | 🔧 | Was: no logging on any auth path — including refresh-token **reuse detection**, the strongest signal of token theft in the system, which vanished silently. **Now**: `ILogger` added to all four Identity command handlers. Login: `Information` on success, `Warning` on failure/lockout (email + IP only — never password or hash). Register: `Information` on success, `Warning` on duplicate-email attempts. Refresh: `Warning` specifically on the reuse-detected/family-revoked branch (family id + user id). Logout: `Information`. |
| 12 | No sensitive data in logs | ✅ | `GlobalExceptionHandler` never surfaces exception internals in 4xx bodies, and 5xx responses stay generic. No exception message anywhere embeds a password, password hash, or raw token. The new logging added for #11 was written to the same rule and reviewed line-by-line — only email, user id, IP, and counts are logged, never a credential or token value. |
| 13 | Remaining OWASP gaps, Phase 1 | ⚠️ | **Password reset / forgot-password**: not built. Wasn't in M1's original endpoint list; needs email infrastructure Phase 1 doesn't have yet. Deferred to a later milestone, not silently dropped. **MFA**: not required by the Phase 1 architecture doc; deferred. **Register-endpoint email enumeration**: `POST /auth/register` returns `409 Conflict` on a duplicate email, which lets a caller confirm an email is registered. **Confirmed with the user**: kept as-is — it's what gives a legitimate user immediate, correct "you already have an account" feedback without an email-verification system Phase 1 doesn't have; the real fix (ambiguous response + a "check your inbox" email flow) is deferred alongside password reset. Documented here as an accepted, industry-common trade-off rather than silently left unexamined. **JWT "none"/algorithm-confusion attacks**: mitigated — `TokenValidationParameters.IssuerSigningKey` is fixed server-side and the validation library doesn't trust an attacker-supplied algorithm from the token header. |

## Configuration reference

All four new/touched settings live in `appsettings.json` (safe defaults, no secrets) and are
overridable per-environment the same way `Jwt:*` already is:

```json
"Lockout": { "MaxFailedAttempts": 5, "LockoutDurationMinutes": 15 },
"PasswordPolicy": { "MinimumLength": 8, "MaximumLength": 128 },
"RateLimiting": { "PermitLimit": 20, "WindowSeconds": 60 }
```

## Verification performed

- Full backend test suite: 28 tests green (7 Domain, 8 Application, 13 Api.Integration — 10
  original auth flows + 2 new account-lockout tests + 1 new rate-limit test).
- New `AccountLockoutTests`: confirms 5 failed logins lock the account (6th attempt fails even
  with the correct password), and confirms a successful login resets the failure counter.
- New `RateLimitingTests`: a dedicated low-threshold test factory confirms the 4th request within
  a fixed window returns `429`.
- Manual: decoded an issued access token and confirmed only `sub`/`jti`/`iss`/`aud`/`exp` are
  present — no `email`/`name`.
- Manual: confirmed response headers include `X-Content-Type-Options: nosniff` and
  `X-Frame-Options: DENY` on a live request.

## Explicitly out of scope for this pass

Nothing beyond the auth surface already built in M1 was touched — no new endpoints, no Google
OAuth (M2), no email infrastructure. The three deferred items in row 13 remain open and are
recorded here so they aren't rediscovered from scratch when they do get picked up.
