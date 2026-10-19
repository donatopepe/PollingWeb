# PollingWeb — Iteration TODO

## Goal
Test the pure recurring daily JobDataMap projection for supplier jobs, asserting its exact legacy three-key contract.

## Metric / threshold
Projection asserts `codist`, `idprocesso`, `onetime=false`, excludes `tipoExport`; focused/full tests and both PollingClass TFMs pass; diff clean. No scheduler runtime.

## Ordered steps
1. Confirm main clean, CodeGraph, task order, review supplier map and tests. **Done.**
2. Baseline: supplier daily uses shared projection; no test explicitly asserts its exact key set and recurring value. **Done.**
3. Add supplier daily map contract coverage. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused supplier policy 17/17; suite 235 pass/2 skip; diff check exit 0. **Done.**
5. Update records and commit/push. **In progress.**

## Constraints
No Quartz/job/DB execution; only test policy.

## Acceptance
- [ ] Supplier three-key projection asserted exactly.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO evidence recorded, commit pushed, tree clean.
