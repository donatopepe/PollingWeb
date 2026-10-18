# PollingWeb — Iteration TODO

## Goal
Unify the existing daily trigger override/duplicate decision across the invoice and SQL01 gateways using the policy already tested for orders.

## Metric / threshold
Invoice and SQL01 gateways use `ShouldUnscheduleExistingTrigger` and `CanScheduleDailyTrigger` with preserved call order; focused tests, both PollingClass builds, full unit suite pass. No scheduler/DB.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan, order policy and remaining gateway conditions. **Done.**
2. Baseline: order and budget use shared override policy; invoice and SQL01 still duplicate boolean logic/duplicate checks. **Done.**
3. Wire policy into invoice and SQL01 daily branches without behavior change. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused invoice/SQL01 tests 17/17; unit suite 216 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit and push. **In progress.**

## Constraints
Preserve exact sequencing and duplicate semantics; don't instantiate scheduler or access DB.

## Acceptance
- [ ] Invoice and SQL01 gateways reuse the tested pure decisions.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG evidence truthful; main updated cleanly.
