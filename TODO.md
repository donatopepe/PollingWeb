# PollingWeb — Iteration TODO

## Goal
Continue T3 with one isolated synthetic SQL integration assertion, without changing SQL definitions.

## Metric / threshold
One uncovered behavior verified against both deployed function copies; IntegrationTests build exit 0; focused SQL test 1/1 pass; `git diff --check` exit 0.

## Ordered steps
1. Confirm clean main, index, plan/log and inspect test + SQL source. **Done.**
2. Gap: typographic apostrophe `’` was tested in a combined input only; isolate its exact replacement behavior using synthetic plain text. **Done.**
3. Add one expression and exact assertion to shared test. **Done.**
4. First build exposed missing C# concatenation operator; fixed. Build exit 0; focused SQL test 1/1 pass; diff check exit 0. **Done.**
5. Update PLAN §8 and LOOP_LOG, commit/push. **In progress.**

## Constraints
No SQL/domain changes, only synthetic data, password only in process environment; no Docker image pulls or unsupported-platform claims.

## Acceptance
- [x] Both `StripHTML` functions replace isolated U+2019 with one space.
- [x] Build, focused test, and whitespace check pass.
- [x] PLAN §8/LOOP_LOG record real evidence.
- [ ] T3 conventional commit pushed; clean main.
