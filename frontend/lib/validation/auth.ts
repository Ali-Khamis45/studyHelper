import { z } from "zod";

export const registerSchema = z.object({
  displayName: z.string().min(1, "Name is required").max(200),
  email: z.email("Enter a valid email address"),
  password: z.string().min(8, "Must be at least 8 characters").max(128),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.email("Enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
