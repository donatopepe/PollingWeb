# PollingWeb — Iteration TODO

## Goal
Continue T3 by completing a missing assertion for an already-executed synthetic SQL input; do not change SQL definitions.

## Metric / threshold
The focused integration test asserts every selected result column for both function copies; build and focused test pass; diff check passes.

## Ordered steps
1. Confirm clean main and inspect current T3 plan/log/test. **Done.**
2. Baseline gap: query includes isolated apostrophe at result ordinal 26, but the test has no assertion for that column (indices jump from 25 to 27). Expected output is one space per the SQL `REPLACE` rule. **Done.**
3. Add the missing assertion for result column 26. **Done.**
4. Build exit 0 (12 existing warnings); focused SQL test 1/1 pass; `git diff --check` exit 0. **Done.**
5. Record actual results and commit/push. **In progress.**

## Constraints
No SQL/domain modifications, only synthetic test data, credential kept in process environment, no image pulls or unsupported OS claims.

## Acceptance
- [x] Existing `N''''` expression's result is asserted as a single space for both function copies.
- [x] Build, focused SQL test, and `git diff --check` succeed.
- [x] PLAN §8 and LOOP_LOG contain actual evidence.
- [ ] T3 conventional commit pushed; clean main.
