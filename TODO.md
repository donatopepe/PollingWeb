# PollingWeb — Iteration TODO

## Goal
Add offline exact-key coverage for the SQL01 daily JobDataMap contract, including recurring flag and omission of `tipoExport`.

## Metric / threshold
Policy projection is asserted to have exactly `codist`, `idprocesso`, `onetime` with expected values. Both TFMs/focused/full tests/diff check pass. No production code or scheduler execution.

## Ordered steps
1. Confirm clean main, CodeGraph, plan order, inspect SQL01 tests/projection. **Done.**
2. Baseline: SQL01 daily uses shared projection but exact-key test has not been added to SQL01 adapter tests. **Done.**
3. Add the focused SQL01 daily map contract test. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused SQL01/order policy 44/44; suite 234 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG/TODO, commit/push and verify clean. **In progress.**

## Constraints
No scheduler/job/database runtime; no functional modifications.

## Acceptance
- [ ] Exact SQL01 daily map key/value contract covered.
- [ ] Builds/tests/diff check pass.
- [ ] Evidence recorded, commit pushed, clean tree verified.
