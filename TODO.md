# PollingWeb — Iteration TODO

## Goal
Reuse the existing shared JobDataMap policy for the SQL01 interval gateway, preserving its three keys and interval-derived `onetime` value.

## Metric / threshold
SQL01 interval map created via tested pure projection for zero and positive intervals; no additional key or value change. Both TFM builds, focused/full unit tests, diff check pass; no scheduler/DB execution.

## Ordered steps
1. Check clean main, CodeGraph, PROMPT/PLAN, current gateway and tests. **Done.**
2. Baseline: order interval uses projection; SQL01 interval still writes the same three keys directly in branches. **Done.**
3. Wire SQL01 interval map through `IntervalJobData`, omit `tipoExport`; added zero/positive projection cases. **Done.**
4. PollingClass net8/net48 exit 0, 0 warnings each; focused SQL01/order tests 42/42; suite 230 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit/push and verify clean. **In progress.**

## Constraints
No Quartz scheduler/job or DB; preserve validation, trigger identity, timing and exact map keys.

## Acceptance
- [ ] SQL01 interval uses pure projection with exact legacy keys and onetime semantics.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; commits pushed; clean tree.
