# PollingWeb — Iteration TODO

## Goal
Continue T3 by validating one additional synthetic SQL function behavior; preserve SQL definitions and domain semantics.

## Metric / threshold
One previously untested text character case is asserted against both deployed `StripHTML` copies; IntegrationTests build and filtered SQL test succeed; `git diff --check` succeeds.

## Ordered steps
1. Confirm clean `main`, read task/log/current function test. **Done.**
2. Baseline gap: tilde `~` in plain text is not tested; it is not among the SQL replacement characters, so expect `A~B` unchanged. **Done.**
3. Add one synthetic input and exact assertion to the shared query. **Done.**
4. Initial build found missing C# concatenation operator; fixed. Build exit 0, focused SQL test 1/1 pass, diff check exit 0. **Done.**
5. Record actual evidence, commit, push. **In progress.**

## Constraints
No SQL edits, no production data, credentials only in process environment, no image pulls or claims for unavailable platforms.

## Acceptance
- [x] Both function copies preserve `A~B` unchanged.
- [ ] Build, focused SQL test, and diff check pass.
- [x] PLAN §8 and LOOP_LOG document real outcomes.
- [ ] Atomic T3 conventional commit is pushed and `main` is clean.
