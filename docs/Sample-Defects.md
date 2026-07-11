# Defect examples to practise

1. Employee can approve their own leave — expected 403, actual 200.
2. End date before start date accepted — expected 400.
3. Duplicate overlapping request created — expected 409.
4. Approval reduces three-day leave by only two days.
5. pageSize=10 returns more than ten items.
6. API returns stack trace or password field.

Tip for the developer: introduce one defect on a separate Git branch, let the tester report it, then merge the fix after retesting.
