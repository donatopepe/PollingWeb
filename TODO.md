# PollingWeb — Iteration TODO

## Goal
Fix X2.3 JobSQL01 recurring schedule contract: daily SQL01 jobs must set the false one-time flag consumed by Execute.

## Metric / threshold
Daily SQL01 policy explicitly yields false; policy unit test, both PollingClass TFM builds, focused/full unit tests and diff check pass. No scheduler/database execution.

## Ordered steps
1. Confirm main/CodeGraph/PROMPT/PLAN and inspect JobSQL01 + SQL01 gateway/tests. **Done.**
2. Baseline: daily gateway omits `onetime`; JobSQL01.Execute converts the missing JobDataMap value to bool. **Done.**
3. Add reusable daily recurring flag policy and test, apply to SQL01 gateway. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused SQL01 tests 8/8; unit suite 187 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit and push. **In progress.**

## Constraints
No database credentials discovery, no production schedule writes, no changes to job/domain behavior besides required flag.

## Acceptance
- [ ] Daily SQL01 job passes `onetime=false`.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG evidence current; conventional commit pushed and tree clean.
