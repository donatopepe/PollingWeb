# PollingWeb — Iteration TODO

## Goal
Add a non-side-effecting test of the recurring-vs-one-time Quartz job-data policy used across X2.3 gateways.

## Metric / threshold
Tests prove the shared recurring value converts to false and explicit one-time value to true, consistent with each job's Execute logic; focused/full tests and both PollingClass builds pass. No Quartz scheduler or DB started.

## Ordered steps
1. Verify main, CodeGraph, PROMPT/PLAN and current gateway/job contract. **Done.**
2. Baseline: daily gateways use shared false policy; interval gateways explicitly set true for zero and false for positive delay. Existing tests only assert daily false. **Done.**
3. Add an interval policy and test for the contrasting true/false values. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused WorkerJobContractTests 31/31 pass; unit suite 198 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit and push. **In progress.**

## Constraints
No database/scheduler invocation, no credentials, no change to interval semantics.

## Acceptance
- [ ] Tests assert true for zero-interval one-time and false for positive interval.
- [ ] Builds and all applicable tests pass; diff check clean.
- [ ] PLAN/LOOP_LOG reflect actual results; changes pushed.
