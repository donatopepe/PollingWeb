# PollingWeb — Iteration TODO

## Goal
Add explicit pure policy coverage for the budget job's `onetime` boolean conversion consumed by the legacy invocation.

## Metric / threshold
A tested conversion policy maps Quartz strings `true/false` to booleans exactly as `Convert.ToBoolean`; `JobBudget` uses it. No invocation/DB execution; both TFMs and tests pass.

## Ordered steps
1. Confirm clean main, CodeGraph, plan order and inspect JobBudget/current contracts. **Done.**
2. Baseline: callers directly `Convert.ToBoolean` on JobDataMap values; no pure dispatch-policy test pins it specifically for budget. **Done.**
3. Extract boolean conversion policy and use in JobBudget; test both values. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); WorkerJobContractTests 49/49; suite 248 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit/push, verify clean. **In progress.**

## Constraints
No budget job execution, database or scheduler; preserve conversion semantics and legacy method signature.

## Acceptance
- [ ] JobBudget uses tested policy; true/false semantics match.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; pushed cleanly.
