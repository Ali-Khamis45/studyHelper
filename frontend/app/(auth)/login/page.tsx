"use client";

import { useEffect, useState, useSyncExternalStore } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { login as loginRequest } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";
import { useAuthStore } from "@/lib/stores/authStore";
import { loginSchema, type LoginFormValues } from "@/lib/validation/auth";

const REMEMBERED_EMAIL_KEY = "aistudyos.rememberedEmail";
const emptySubscribe = () => () => {};

export default function LoginPage() {
  const router = useRouter();
  const setSession = useAuthStore((state) => state.setSession);
  const [rememberMe, setRememberMe] = useState(true);

  // Reads localStorage through useSyncExternalStore rather than an effect + setState — that keeps
  // the server/first-paint render and the post-hydration render consistent without a state update
  // sneaking in after mount.
  const rememberedEmail = useSyncExternalStore(
    emptySubscribe,
    () => window.localStorage.getItem(REMEMBERED_EMAIL_KEY),
    () => null,
  );

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  useEffect(() => {
    if (rememberedEmail) setValue("email", rememberedEmail);
  }, [rememberedEmail, setValue]);

  const mutation = useMutation({
    mutationFn: loginRequest,
    onSuccess: (data, variables) => {
      if (rememberMe) window.localStorage.setItem(REMEMBERED_EMAIL_KEY, variables.email);
      else window.localStorage.removeItem(REMEMBERED_EMAIL_KEY);
      setSession(data.user, data.accessToken);
      router.push("/dashboard");
    },
  });

  return (
    <div className="flex flex-col gap-8">
      <div className="flex flex-col gap-2">
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back</h1>
        <p className="text-sm text-muted-foreground">Sign in to continue to your mentor.</p>
      </div>

      <form
        className="flex flex-col gap-5"
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@example.com"
            className="h-11 rounded-xl text-base"
            {...register("email")}
          />
          {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
        </div>

        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="password">Password</Label>
            <span
              className="cursor-not-allowed text-xs text-muted-foreground/70"
              title="Password reset isn't available yet"
            >
              Forgot password?
            </span>
          </div>
          <Input
            id="password"
            type="password"
            autoComplete="current-password"
            placeholder="••••••••"
            className="h-11 rounded-xl text-base"
            {...register("password")}
          />
          {errors.password && <p className="text-sm text-destructive">{errors.password.message}</p>}
        </div>

        <label className="flex items-center gap-2 text-sm text-muted-foreground select-none">
          <Checkbox checked={rememberMe} onCheckedChange={(checked) => setRememberMe(checked === true)} />
          Remember me on this device
        </label>

        {mutation.isError && (
          <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {mutation.error instanceof ApiError && mutation.error.status === 401
              ? "Invalid email or password."
              : "Something went wrong. Please try again."}
          </p>
        )}

        <Button type="submit" size="lg" className="h-11 w-full rounded-xl text-base" disabled={mutation.isPending}>
          {mutation.isPending ? "Signing in…" : "Sign in"}
        </Button>
      </form>

      <div className="flex items-center gap-3 text-xs text-muted-foreground">
        <span className="h-px flex-1 bg-border" />
        or continue with
        <span className="h-px flex-1 bg-border" />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <Button type="button" variant="outline" className="h-11 rounded-xl" disabled title="Coming soon">
          Google
        </Button>
        <Button type="button" variant="outline" className="h-11 rounded-xl" disabled title="Coming soon">
          GitHub
        </Button>
      </div>

      <p className="text-center text-sm text-muted-foreground">
        Don&apos;t have an account?{" "}
        <Link href="/register" className="font-medium text-foreground underline underline-offset-4">
          Create one
        </Link>
      </p>
    </div>
  );
}
