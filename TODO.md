# PollingWeb — Iteration TODO

## Goal
Make task-type mapping immutable so callers cannot mutate a validated shared routing result after policy creation.

## Metric / threshold
`ConfiguredTaskMapping` immutable (get-only properties, constructor-controlled), all existing type 1–7 mapping and helper behavior intact; unit tests/builds/diff check pass.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN and inspect mapping type and consumers. **Done.**
2. Baseline: `MapTaskType` returns mutable public properties, allowing caller mutation after lookup. **Done.**
3. Make mapping immutable with private setters and internal constructor; existing assertions cover all mappings. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); WorkerJobContractTests 40/40; suite 237 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence; commit/push and confirm clean. **In progress.**

## Constraints
No change to task mapping values, exceptions or routing; no DB/job execution.

## Acceptance
- [ ] Mapping instances immutable after creation.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated and pushed cleanly.
