# PollingWeb — Iteration TODO

## Goal
Reuse the pure shared job-data projection for the supplier gateway's daily schedule, without altering its Quartz identities, override or trigger behavior.

## Metric / threshold
Fornitori daily job map is constructed through shared policy, retaining the legacy three keys and `onetime=false`. Both TFMs and relevant/full tests pass; no scheduler or DB runtime.

## Ordered steps
1. Confirm clean main, CodeGraph, PROMPT/PLAN order, inspect supplier daily map. **Done.**
2. Baseline: other daily gateways use projection; supplier daily still adds three values inline. **Done.**
3. Wire supplier daily to projection, omit `tipoExport`. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused supplier/job-contract 56/56; full suite 228 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push, confirm clean tree. **In progress.**

## Constraints
No scheduler start, job execution, SQL/SAP access; preserve exact keys and values.

## Acceptance
- [ ] Supplier daily map uses tested projection with exact key set/values.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated and push confirmed.
