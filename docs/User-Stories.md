# User Stories and Acceptance Criteria

## US-01 Login
As a registered user, I want to log in so that I can access APIs permitted for my role.

Acceptance criteria:
- Valid credentials return 200 and a JWT.
- Invalid credentials return 401.
- Email is mandatory and must be valid.
- Password is mandatory.

## US-02 Submit leave request
As an employee, I want to submit leave for future dates.

Acceptance criteria:
- Only an authenticated employee can submit.
- Start date cannot be in the past.
- End date cannot be before start date.
- Weekends are excluded from numberOfDays.
- Overlapping active requests return 409.
- Insufficient balance returns 409.
- A successful request returns 201 and PENDING status.

## US-03 Manager decision
As a manager, I want to approve or reject requests from my direct reports.

Acceptance criteria:
- Only the assigned manager can decide.
- Only PENDING requests can be decided.
- Approval reduces the matching leave balance.
- Rejection does not reduce balance.

## US-04 Cancel leave
As an employee, I want to cancel my own leave.

Acceptance criteria:
- The employee cannot cancel another employee's request.
- Rejected or already cancelled requests cannot be cancelled.
- Approved leave can be cancelled only before start date.
- Cancelling approved leave restores balance.
