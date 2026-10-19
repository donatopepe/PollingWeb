# PollingWeb — Iteration TODO

## Goal
Reuse the already-tested one-time string parser in the invoice job adapter without changing its legacy invocation.

## Metric / threshold
`JobImportFatture` calls `FornitoriJobDispatchPolicy.ParseOneTime`; string `true/false` mapping covered; method branch and arguments unchanged. Both TFMs, focused/full tests, diff check pass; no job execution.

## Ordered steps
1. Check clean main, CodeGraph, plan order, inspect invoice job parser and shared policy. **Done.**
2. Baseline: budget and SQL01 use shared parser; invoice job still calls `Convert.ToBoolean` inline. **Done.**
3. Replace invoice parse call with shared policy; existing true/false policy tests suffice. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused job/invoice tests 59/59; suite 248 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push, verify clean. **In progress.**

## Constraints
No job/SAP/DB execution; preserve ONETIME vs recurring calls and parameters.

## Acceptance
- [ ] Invoice job uses tested shared parser.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated and push verified clean.
