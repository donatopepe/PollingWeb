# PollingWeb — Iteration TODO

## Goal
Reuse the tested pure interval JobDataMap projection for the invoice interval gateway, preserving three legacy fields and one-time flag.

## Metric / threshold
Invoice interval uses shared projection without `tipoExport`; zero→`true`, positive→`false`; job/trigger identities, timing, validation unchanged. Both TFM builds, focused/full tests and diff check pass; no scheduler/DB runtime.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan and inspect invoice interval path. **Done.**
2. Baseline: budget and SQL01 interval maps use projection; invoice interval still duplicates field writes. **Done.**
3. Wire invoice interval through shared projection. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused invoice/order policy 43/43; suite 230 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit/push, verify clean. **In progress.**

## Constraints
No job/scheduler/DB execution; do not alter generated identities or trigger timing.

## Acceptance
- [ ] Invoice interval uses projection, exact three-key contract.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; push confirmed clean.
