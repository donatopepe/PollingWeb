# PollingWeb — Iteration TODO

## Goal
Continue T3 by completing a missing assertion for an already-executed synthetic SQL input; do not change SQL definitions.

## Metric / threshold
The focused integration test asserts every selected result column for both function copies; build and focused test pass; diff check passes.

## Ordered steps
1. Confirm clean main and inspect current T3 plan/log/test. **Done.**
2. Baseline gap: `StripHTML` does not replace percent signs, but percent preservation in plain text is uncharacterized. Expected `100%` remains unchanged. **Done.**
3. Add one synthetic plain-text input with `%` and exact result assertion for both copies. **Done.**
4. Build exit 0 (12 existing warnings); focused SQL test 1/1 pass; `git diff --check` exit 0. **Done.**
5. Record actual results and commit/push. **In progress.**

## Constraints
No SQL/domain modifications, only synthetic test data, credential kept in process environment, no image pulls or unsupported OS claims.

## Acceptance
- [x] Both function copies preserve the percent sign in plain-text input `100%`.
- [x] Build, focused SQL test, and `git diff --check` succeed.
- [x] PLAN §8 and LOOP_LOG contain actual evidence.
- [ ] T3 conventional commit pushed; clean main.
