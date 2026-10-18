# PollingWeb — Iteration TODO

## Goal
Strengthen X2.3 trigger-policy test coverage for duplicate daily order triggers without touching database or Quartz runtime.

## Metric / threshold
Pure policy exposes both override decision and duplicate-trigger acceptance outcome: existing+override permits replacement; existing+no override rejects; absent trigger permits scheduling. Unit tests/builds pass; no scheduler started.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan and existing order gateway policy. **Done.**
2. Baseline: `ShouldUnscheduleExistingTrigger` only models removal decision; duplicate guard's expected result is untested. **Done.**
3. Add pure policy for duplicate-trigger acceptance and table-driven tests; wire gateway guard. **Done.**
4. Both builds exit 0 (12 warnings each); focused order tests 22/22 pass; unit suite 216 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit and push. **In progress.**

## Constraints
Preserve behavior exactly, no scheduler/database or trigger creation.

## Acceptance
- [ ] Policy models duplicate guard for override/presence combinations.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG actual evidence; commits pushed, tree clean.
