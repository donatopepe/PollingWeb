# PollingWeb — Iteration TODO

## Goal
Add offline contract coverage for order-job execution arguments (one-time flag and export selector) as passed by Quartz job adapters.

## Metric / threshold
A pure adapter mapping is tested for receipt/send, onetime value and export selector; `JobOrdini` uses it with legacy method signatures unchanged. PollingClass both TFMs and unit suite pass; no DB/SAP jobs run.

## Ordered steps
1. Confirm clean main, CodeGraph, PROMPT/PLAN, and inspect `JobOrdini`/existing contract tests. **Done.**
2. Baseline: adapters call legacy methods using onetime and tipoExport; current tests pin mapping families but not argument tuple. **Done.**
3. Add tested pure argument policy and wire both order jobs. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused order policy tests 26/26; unit suite 222 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit, push, verify clean. **In progress.**

## Constraints
No live job execution, SAP/file/database side effects; preserve signatures and values.

## Acceptance
- [ ] Mapping preserves send/receipt arguments and nullable selector semantics.
- [ ] Build/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated and pushed.
