# PollingWeb — Iteration TODO

## Goal
Close a demonstrable X2.3 source-contract gap: assert all six Quartz job classes consume the required `codist`, `idprocesso`, and `onetime` fields from JobDataMap.

## Metric / threshold
Each job source contains reads for these three keys; order-send also reads `tipoExport`. Test source contract across all six job types; builds and unit suite pass.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan and inspect each Execute body. **Done.**
2. Baseline: tests assert job signature, persistence attribute and error handler, but don't guard JobDataMap key reads in source. **Done.**
3. Add source contract assertions for required keys and order-send selector. **Done.**
4. Focused six-job source contract 6/6 pass; PollingClass net8 build exit 0/0 warnings; net48 exit 0/12 warnings; unit suite 210 pass/2 skip; diff check exit 0. **Done.**
5. Update plan/log, commit and push. **In progress.**

## Constraints
No domain jobs, DB, scheduler or real data executed.

## Acceptance
- [ ] Source tests enforce required keys per job.
- [ ] Builds and tests pass; diff check clean.
- [ ] PLAN/LOOP_LOG truthful and commits pushed.
