# PollingWeb — Iteration TODO

## Goal
Reuse tested pure job-data projection in SQL01 daily gateway while preserving its legacy three-key map.

## Metric / threshold
SQL01 daily creates `codist`, `idprocesso`, `onetime=false`, excludes order-only `tipoExport`; job type, identity, cron and override semantics unchanged. Builds/tests/diff check pass; no runtime scheduler/DB.

## Ordered steps
1. Confirm clean main, CodeGraph, PROMPT/PLAN order, inspect SQL01 daily map. **Done.**
2. Baseline: order, budget and invoice use tested projection; SQL01 writes recurring three keys directly. **Done.**
3. Wire SQL01 daily map through shared projection, omit `tipoExport`. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused SQL01/order tests 40/40; unit suite 228 pass/2 skip; diff check exit 0. **Done.**
5. Log evidence, commit and push; verify clean. **In progress.**

## Constraints
No scheduler/job/DB; preserve map keys and scheduling behavior.

## Acceptance
- [ ] SQL01 uses tested projection with exact key set and values.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO current; pushed, tree clean.
