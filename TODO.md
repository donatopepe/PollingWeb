# PollingWeb — Iteration TODO

## Goal
Adopt the tested shared `ParseOneTime` helper in both order jobs, removing the remaining inline conversions without altering call arguments.

## Metric / threshold
Receipt/send jobs use the shared parser; existing argument policies and true/false tests remain green. Both TFM builds, focused/full unit tests and diff check pass. No jobs executed.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN order, inspect order job conversions. **Done.**
2. Baseline: supplier, budget, invoice and SQL01 use shared parser; order jobs still use inline `Convert.ToBoolean`. **Done.**
3. Replace both order conversions with shared helper; no other behavior changes. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused order/worker tests 86/86; suite 248 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push; verify clean. **In progress.**

## Constraints
No order job/SAP/DB execution; preserve receipt/send routing and arguments.

## Acceptance
- [ ] Both order job classes use shared parser.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated, pushed and clean.
