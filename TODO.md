# PollingWeb — Iteration TODO

## Goal
Reuse the tested interval JobDataMap projection for ImportBudget interval schedules, preserving the three legacy keys and interval flag.

## Metric / threshold
Budget interval uses `IntervalJobData` (no `tipoExport`), zero→`true`, positive→`false`; validation, trigger identity/timing unchanged. Both TFM builds, focused/full tests, diff check pass; no scheduler/DB runtime.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN and inspect budget interval path. **Done.**
2. Baseline: SQL01 interval uses projection; budget interval writes the same three fields directly in each branch. **Done.**
3. Wire budget interval through projection, omitting order-only selector. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused budget/order tests 42/42; suite 230 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit/push, verify clean. **In progress.**

## Constraints
No scheduler/job/DB execution; preserve identity, validation and trigger timing.

## Acceptance
- [ ] Budget interval uses shared policy and exact key/value contract.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; push and clean tree verified.
