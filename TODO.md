# PollingWeb — Iteration TODO

## Goal
Add offline job execution contract coverage for JobFornitori and JobImportFatture ONETIME-vs-recurring dispatch without executing legacy work or SQL.

## Metric / threshold
Each job's dispatch policy is testable for onetime true/false and the existing adapter path preserves methods/arguments; builds, focused/full tests and diff check pass. No DB/scheduler execution.

## Ordered steps
1. Check clean main, CodeGraph, PROMPT/PLAN order, job implementations and current contract tests. **Done.**
2. Baseline: two jobs choose distinct legacy methods based on `onetime`; tests only validate JobDataMap boolean semantics and do not pin dispatch choice. **Done.**
3. Extract pure dispatch selector and test both job families/boolean branches; wire jobs. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused contract tests 40/40; suite 218 pass/2 skip; diff check exit 0. **Done.**
5. Log actual result, commit, push and confirm clean tree. **In progress.**

## Constraints
No execution of jobs, database access, or changed domain/SAP behavior.

## Acceptance
- [ ] Pure tested selector covers both job paths and flags.
- [ ] Build/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; conventional commit pushed.
