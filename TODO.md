# PollingWeb — Iteration TODO

## Goal
Make task-type/order-selector mapping an atomic pure policy result, so one validated type consistently determines job family and (only for send types) export selector.

## Metric / threshold
A tested `TaskTypeMapping` returns existing family/job mappings for 1–7 and selector ODA for 2, RDA for 6, null otherwise; existing APIs delegate to it. No routing changes. Both TFMs, focused/full tests and diff check pass.

## Ordered steps
1. Verify clean main, CodeGraph, PLAN ordering and inspect existing mapping/tests. **Done.**
2. Baseline: callers separately invoke `JobType`, `ProcessFamily`, `OrderExportType`; their valid task-type sets are currently duplicated. **Done.**
3. Add pure cohesive mapping and tests covering all seven task types; existing policy methods delegate. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); WorkerJobContractTests 40/40; suite 237 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG/TODO; commit/push and verify clean. **In progress.**

## Constraints
No invented mappings or TaskService/SQL assumptions; preserve exact existing behavior.

## Acceptance
- [ ] All task mappings and order selector values match current routing.
- [ ] Both builds/tests/diff check pass.
- [ ] Evidence documented and pushed cleanly.
