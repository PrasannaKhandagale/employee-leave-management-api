# API Test Plan

Scope: authentication, employee access, leave balance, leave submission, manager decision, cancellation, filtering, pagination, security, and error handling.

Test types:
- Functional positive and negative testing
- Validation and boundary testing
- Authentication and authorization testing
- Workflow/end-to-end testing
- Response schema and status-code testing
- Basic response-time checks
- Regression testing using Postman Collection Runner/Newman

Entry criteria:
- API is deployed and /health returns 200.
- Swagger is available.
- Test accounts are seeded.

Exit criteria:
- All critical and high-severity tests pass.
- No open blocker or critical defects.
- Regression collection completes successfully.
