---
applyTo: "backend/OrderFlow.Api/**/*.cs"
---

# API Standards

- Controllers must use DTOs.
- Never expose domain entities directly.
- Controllers must remain thin.
- Business logic belongs in the Application layer.
- Use appropriate HTTP status codes.