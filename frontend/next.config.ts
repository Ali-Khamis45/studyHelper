import type { NextConfig } from "next";

const backendOrigin = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5299";

const nextConfig: NextConfig = {
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
