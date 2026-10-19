# PollingWeb — Iteration TODO

## Goal
Cover exact daily invoice JobDataMap contract with an offline test, including omission of order-only `tipoExport`.

## Metric / threshold
Test projection used by invoice daily: exact three keys `codist`, `idprocesso`, `onetime`, values and recurring false. Both TFMs/focused/full tests/diff check pass; no runtime scheduler.

## Ordered steps
1. Verify clean main, CodeGraph, plan order and invoice adapter/policy tests. **Done.**
2. Baseline: invoice uses pure map projection but tests don't assert its exact three-key daily shape. **Done.**
3. Add exact invoice daily projection contract test. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused invoice/order policy 45/45; suite 233 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push; verify clean. **In progress.**

## Constraints
No scheduler/job/DB; preserve behavior and no public API change.

## Acceptance
- [ ] Invoice daily map exact key/value contract tested.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated, pushed and clean.
