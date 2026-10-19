# PollingWeb — Iteration TODO

## Goal
Add pure argument-selection coverage for `JobOrdini_SAP_Esito` receipt jobs, confirming `tipoExport` is discarded while one-time state is preserved.

## Metric / threshold
Receipt arguments for both oneTime values always have `SendFile=false`, `TipoExport=""`, with OneTime unchanged; focused/full tests, both TFM builds and diff check pass. No job execution.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan order; inspect order jobs and policy tests. **Done.**
2. Baseline: send/receipt argument policy tested, but no explicit test asserts selector discarded for receipt for both flags. **Done.**
3. Add two table cases for receipt selector normalization. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused order policy 37/37; suite 246 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence; commit/push; verify clean. **In progress.**

## Constraints
Pure tests only; no SAP/job/DB execution, no behavior changes.

## Acceptance
- [ ] Receipt arguments pinned for oneTime true/false and selector discarded.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated, pushed, clean.
