# PollingWeb — Iteration TODO

## Goal
Add focused coverage of budget gateway's daily JobDataMap contract: exact three-key map and recurring flag, without executing Quartz.

## Metric / threshold
Pure policy projection emits `codist`, `idprocesso`, `onetime=false` for budget daily and excludes `tipoExport`; existing tests explicitly assert this. Both TFM builds, focused/full tests and diff check pass.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN order; inspect budget policy and adapter tests. **Done.**
2. Baseline: shared policy supports omission but tests assert exact keys only for order; budget adapter tests don't pin daily JobDataMap shape. **Done.**
3. Add exact-key/value test for budget map projection. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused budget/order policy 44/44; suite 232 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit/push, verify clean. **In progress.**

## Constraints
No scheduler/job/DB execution; preserve existing behavior.

## Acceptance
- [ ] Budget projection asserts exact three keys and recurring false.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; pushed; clean tree verified.
