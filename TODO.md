# PollingWeb — Iteration TODO

## Goal
Preserve the distinct legacy order export selector in X2.3 worker job mapping: TaskService type 2 is ODA, type 6 is RDA.

## Metric / threshold
Pure policy returns the exact established selector for both order task types and rejects unrelated types; focused unit tests/builds pass with no job execution or database dependency.

## Ordered steps
1. Confirm main, CodeGraph, prompt/plan, prior iteration and WebForms type routing. **Done.**
2. Baseline: types 2 and 6 share `JobOrdini_SAP_Invio`, but `TaskService.aspx.cs` passes different `tipoExport` values (`ODA`/`RDA`); prior policy only mapped job type. **Done.**
3. Add a pure export selector and table-driven assertions. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused contracts 22/22 pass; unit suite 184 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit and push. **In progress.**

## Constraints
No secrets discovery, no DB login guessing, no fabricated domain schedules, preserve ordered plan.

## Acceptance
- [ ] X2.1 marked [!] with blocker and remediation path.
- [ ] PLAN and LOOP_LOG truthful; diff check passes.
- [ ] Atomic documentation commit pushed; main clean.
