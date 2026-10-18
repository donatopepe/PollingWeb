# PollingWeb — Iteration TODO

## Goal
Remove duplicate unschedule policy coverage and use the already-tested pure decision in one more gateway where behavior matches.

## Metric / threshold
Budget daily override path calls `ShouldUnscheduleExistingTrigger`; policy test covers both values; existing duplicate guard remains intact. Both PollingClass TFMs and unit tests pass without scheduler/DB.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan and inspect gateway override branches/tests. **Done.**
2. Baseline: order policy already tested, but ImportBudget gateway still has direct override conditional and same unschedule behavior. **Done.**
3. Wire the shared pure decision into ImportBudget daily path; reuse existing policy tests (true/false) rather than duplicate them. **Done.**
4. Both PollingClass builds exit 0 (12 warnings each); focused Budget tests 8/8; unit suite 216 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG and commit/push. **In progress.**

## Constraints
No scheduler or SQL, preserve operation order, no database credentials.

## Acceptance
- [ ] Budget daily override path uses shared policy, no side effects in tests.
- [ ] Both builds/tests/diff check pass.
- [ ] Actual evidence logged and commit pushed.
