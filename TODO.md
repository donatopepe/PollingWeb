# PollingWeb — Iteration TODO

## Goal
Fix X2.3 order daily Quartz jobs: both `JobOrdini_SAP_*` read `onetime`, while `ScheduleDaily` currently omits it.

## Metric / threshold
Daily order JobDataMap sets `onetime=false` for send and receipt jobs; policy-level unit test plus both PollingClass TFM builds and unit suite pass. No DB/scheduler invocation.

## Ordered steps
1. Verify main, CodeGraph, PROMPT/PLAN and inspect order jobs/gateway. **Done.**
2. Baseline: both `JobOrdini_SAP_Esito.Execute` and `JobOrdini_SAP_Invio.Execute` convert JobDataMap `onetime`; daily gateway omits it. **Done.**
3. Add shared daily recurring policy and test; apply to daily gateway only. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused order tests 11/11 pass; unit suite 188 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit and push. **In progress.**

## Constraints
No DB access/credentials discovery or production schedules; preserve ODA/RDA routing and job semantics.

## Acceptance
- [ ] Daily order jobs have explicit `onetime=false`.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG actual evidence recorded; commit pushed and main clean.
