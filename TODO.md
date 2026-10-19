# PollingWeb — Iteration TODO

## Goal
Assert SQL01 interval map's exact legacy keys and one-time values in focused pure-policy tests.

## Metric / threshold
Test `IntervalJobData` for zero and positive intervals; assert exact three keys, passed identifiers and `onetime=true/false`. Builds/tests/diff check pass, no scheduler execution.

## Ordered steps
1. Check clean main, CodeGraph, PROMPT/PLAN and SQL01 interval projection/tests. **Done.**
2. Baseline: interval projection semantics covered in order policy tests but SQL01's dedicated tests only check interval validity. **Done.**
3. Add SQL01 interval key/value test cases. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); SQL01 tests 11/11; unit suite 237 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push and verify clean. **In progress.**

## Constraints
No production changes, no Quartz/DB/job run.

## Acceptance
- [ ] SQL01 interval projection exact map tested for zero/positive.
- [ ] Builds/tests/diff check pass.
- [ ] Evidence recorded and changes pushed, clean tree.
