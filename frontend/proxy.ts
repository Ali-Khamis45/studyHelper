import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Coarse gate only: checks that the httpOnly refresh cookie exists, since Proxy can't validate
// the JWT signature without shipping the signing key here. Real per-request authorization still
// happens server-side via [Authorize] on every API call — this just avoids flashing protected
// pages at users with no session before the (app) layout's client-side bootstrap redirects them.
const REFRESH_COOKIE_NAME = "refreshToken";

export function proxy(request: NextRequest) {
  const hasSession = request.cookies.has(REFRESH_COOKIE_NAME);

  if (!hasSession) {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard/:path*", "/goals/:path*", "/planner/:path*", "/mentor/:path*", "/quiz/:path*", "/analytics/:path*", "/settings/:path*"],
};
