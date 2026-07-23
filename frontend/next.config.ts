import type { NextConfig } from "next";

const backendOrigin = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5299";

const nextConfig: NextConfig = {
  // Next's rewrite proxy defaults experimental.proxyTimeout to 30s (verified directly in
  // node_modules/next/dist/server/lib/router-utils/proxy-request.js) — too short for a real,
  // non-streaming AI recommendation generation on local hardware, which can take 20-40s+. Matches
  // the backend's own Ollama:TimeoutSeconds (120s) so the proxy is never the limiting factor.
  experimental: {
    proxyTimeout: 120_000,
  },
  async rewrites() {
    return [
      {
        source: "/api/backend/:path*",
        destination: `${backendOrigin}/:path*`,
      },
    ];
  },
};

export default nextConfig;
