# PollingWeb — Iteration TODO

## Goal
Use the shared tested Quartz `onetime` parser in `JobFornitori` while preserving its ONETIME/recurring dispatch.

## Metric / threshold
`JobFornitori` uses `FornitoriJobDispatchPolicy.ParseOneTime`; both boolean strings preserve dispatch selection. Both TFMs, focused/full tests, diff check pass. No job execution.

## Ordered steps
1. Confirm clean main, CodeGraph, task order; inspect supplier job parser and tests. **Done.**
2. Baseline: budget, SQL01, invoice use shared parser; supplier still calls `Convert.ToBoolean`. **Done.**
3. Replace parser call in supplier job; rely on shared true/false policy tests. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused job/supplier tests 66/66; suite 248 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push, verify clean. **In progress.**

## Constraints
No supplier job/DB/SAP execution; preserve selected legacy methods and arguments.

## Acceptance
- [ ] Supplier job uses shared parser.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated and push cleanly verified.
