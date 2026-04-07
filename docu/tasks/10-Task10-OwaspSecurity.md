# Task 10: Apply OWASP Security Best Practices

## Status

Pending

## Current Situation

- JWT auth and role-based authorization are in place.
- No explicit OWASP-focused security header policy is configured.
- CORS restriction policy has not been defined.

## Evidence

- src/Presentation/Program.cs

## Remaining Work

1. Add security headers middleware (CSP, X-Content-Type-Options, X-Frame-Options, Referrer-Policy, etc.).
2. Configure restricted CORS policy by allowed origins/methods/headers.
3. Validate via response inspection and document verification checklist.
