# PollingWeb — Iteration TODO

## Goal
Continue T3 with one focused synthetic SQL integration case; do not alter SQL/domain behavior.

## Metric / threshold
Verify one uncovered parser edge against both deployed `StripHTML` functions; project build succeeds, focused SQL test passes, diff check succeeds.

## Ordered steps
1. Confirm clean main and inspect current T3 evidence, test and SQL implementation. **Done.**
2. Gap: hyphen replacement is covered in a combined punctuation phrase but not in plain text isolated from apostrophes/backticks. Expected `x y` for `x-y`. **Done.**
3. Add one synthetic expression and exact assertion to shared test. **Done.**
4. Initial build caught missing concatenation operator; fixed, build exit 0; focused SQL test 1/1 pass; `git diff --check` exit 0. **Done.**
5. Record results in PLAN §8 and LOOP_LOG, commit/push. **In progress.**

## Constraints
Synthetic input only, no SQL changes, no credentials in files/output, no image pulls, no cross-OS/full-solution claim.

## Acceptance
- [x] Both `StripHTML` copies return `x y` for plain text `x-y`.
- [ ] Build, focused test, and `git diff --check` pass.
- [x] PLAN and LOOP_LOG record actual evidence.
- [ ] Atomic T3 commit pushed; clean main.
