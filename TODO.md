# PollingWeb — Iteration TODO

## Goal
Cover the order interval gateway's complete JobDataMap contract (job type, required keys, `onetime`, `tipoExport`) without starting Quartz.

## Metric / threshold
Build a pure job-data projection used by the interval gateway and cover send/receipt + zero/positive interval values, preserving existing behavior and key names. Both TFMs, focused/full tests, diff check pass; no job/DB execution.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN task order and inspect order gateway/tests. **Done.**
2. Baseline: policy tests cover selector normalization and scheduling interval flags separately; no projection test covers full interval JobDataMap contract. **Done.**
3. Add pure interval argument policy and wire into interval gateway. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused order tests 30/30; suite 226 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence and commit/push; confirm clean tree. **In progress.**

## Constraints
Do not instantiate scheduler or run jobs. Preserve property names and semantics exactly.

## Acceptance
- [ ] Policy returns contract values tested for send/receipt and interval flags.
- [ ] net8/net48 builds and tests pass; diff check clean.
- [ ] PLAN/LOOP_LOG/TODO updated; conventional commit pushed.
