# PollingWeb — Iteration TODO

## Goal
Continue T3 with one isolated synthetic SQL integration assertion, leaving function definitions unchanged.

## Metric / threshold
One distinct behavior exercised against both deployed copies; IntegrationTests build succeeds; focused SQL test passes; diff check succeeds.

## Ordered steps
1. Confirm clean main and inspect current T3 specification, log, integration query, and SQL. **Done.**
2. Baseline gap: a bare ampersand in plain text is untested; `StripHTML` has no ampersand replacement, so expect `A&B` unchanged. **Done.**
3. Add one synthetic query expression and exact assertion. **Done.**
4. First build found missing C# concatenation operator; fixed. Build exit 0; focused SQL test 1/1 pass; diff check exit 0. **Done.**
5. Record actual outcomes in PLAN §8 and LOOP_LOG; commit and push. **In progress.**

## Constraints
No SQL/domain changes, synthetic input only, secret stays in process environment, no image downloads or unsupported platform claims.

## Acceptance
- [x] Both `StripHTML` functions preserve `A&B` unchanged.
- [ ] Build, focused SQL test, and `git diff --check` succeed.
- [x] PLAN §8 and LOOP_LOG record actual results.
- [ ] T3 conventional commit pushed; main clean.
