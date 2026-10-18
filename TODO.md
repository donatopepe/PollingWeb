# PollingWeb — Iteration TODO

## Goal
Fix one X2.3 persisted-job contract defect: daily budget Quartz jobs must carry the recurring flag consumed by `JobBudget.Execute`.

## Metric / threshold
Daily budget gateway sets `onetime=false`; a focused unit test verifies the shared recurring-job value; PollingClass builds net8/net48 and unit tests pass. No database/job execution.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan, inspect budget job/gateway/tests. **Done.**
2. Baseline: `JobBudget.Execute` converts `JobDataMap[onetime]`, but `ImportBudgetQuartzGateway.ScheduleDaily` omits that key. **Done.**
3. Add explicit recurring flag using the common policy and test. **Done.**
4. PollingClass net8/net48 builds exit 0, 0 warning/0 errors; focused budget tests 8/8 pass; unit suite 186 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG, commit and push. **In progress.**

## Constraints
No production schedules, SQL login guessing or changes to SAP behavior; credentials only if already authorized in process environment.

## Acceptance
- [ ] Daily budget jobs explicitly pass `onetime=false`.
- [ ] Both TFM builds and focused/full tests pass; diff check clean.
- [ ] Evidence recorded; X2.3 remains partial as appropriate.
- [ ] Commit pushed, main clean.
