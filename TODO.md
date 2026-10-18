# PollingWeb — Iteration TODO

## Goal
Tie each of the six Quartz job source definitions to the shared exception handler call so future edits cannot silently remove required job failure handling.

## Metric / threshold
Contract test inspects each job source and asserts `PollingExceptionHandler.Esegui(ex)` is present; focused/full tests and both PollingClass builds pass. No jobs or database execute.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan and inspect job exception paths. **Done.**
2. Baseline: source grep confirms all six job classes catch `Exception` and call shared handler; tests only checked type/signature, not source contract. **Done.**
3. Extend six-row contract test to assert handler call in each job source. **Done.**
4. Build net8/net48; focused source contract, full unit suite, diff check. **Done.**
5. Update plan/log and commit/push. **In progress.**

## Constraints
No job execution, database, credentials, or production data; do not modify domain error semantics.

## Acceptance
- [ ] Correct coverage gap is tested without duplicate test cases.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG actual outputs recorded and commits pushed.
