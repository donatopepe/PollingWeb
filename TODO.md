# PollingWeb — Iteration TODO

## Goal
Extend the offline X2.3 job contract tests to assert the recurring flag string is parseable as false, matching Quartz `Execute` behavior.

## Metric / threshold
For all six IJob classes, contract test verifies JobDataMap recurring value is the exact boolean string `false` and `Convert.ToBoolean` parses it false; focused/full unit suite passes. No DB/scheduler execution.

## Ordered steps
1. Verify main, CodeGraph, prompt/plan and inspect all job Execute methods and daily gateway data. **Done.**
2. Baseline: all daily gateways now use shared `DailyRecurringOnetimeValue`, but current tests assert the helper literal only rather than the exact conversion consumed by job handlers. **Done.**
3. Add one shared test contract that validates value parse semantics for each job family. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); WorkerJobContractTests 28/28 pass; unit suite 195 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit and push. **In progress.**

## Constraints
No live jobs/database, no credentials; preserve behavior and no guessed data contracts.

## Acceptance
- [ ] Test covers six Quartz job families and conversion false.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN and LOOP_LOG record evidence; commits pushed and tree clean.
