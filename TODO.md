# PollingWeb — Iteration TODO

## Goal
Close another X2.3 JobDataMap contract gap: supplier daily Quartz jobs call `JobFornitori.Execute`, which requires `onetime`.

## Metric / threshold
`FornitoriQuartzGateway.ScheduleDaily` writes the recurring false flag; a focused unit test asserts policy value; both PollingClass target builds and unit suite pass. No DB/scheduler invocation.

## Ordered steps
1. Confirm clean main, CodeGraph, task order, job and supplier gateway implementation. **Done.**
2. Baseline: `JobFornitori.Execute` converts `JobDataMap[onetime]`, but `ScheduleDaily` did not populate it. **Done.**
3. Add explicit recurring flag via shared policy and test. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused supplier tests 16/16 pass; unit suite 189 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG, commit and push. **In progress.**

## Constraints
No real DB operations, no credential discovery, do not change schedules/domain behavior.

## Acceptance
- [ ] Supplier daily jobs receive `onetime=false`.
- [ ] Builds and tests pass; diff check clean.
- [ ] PLAN and LOOP_LOG document observed outputs.
- [ ] Conventional task commit pushed; working tree clean.
