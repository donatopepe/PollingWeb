# PollingWeb — Iteration TODO

## Goal
Verify whether the already-added order export selector tests cover send/receipt/null behavior; do not duplicate tests if present.

## Metric / threshold
Inspect test definitions, build PollingClass net8/net48, focused order policy tests and full unit suite; document actual result. No production/job changes.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan and existing policy tests. **Done.**
2. Baseline: `OrdiniSchedulerPolicyTests` already has five cases: send ODA/RDA, send null→empty, receipt ODA/null→empty. No test gap for that policy. **Done.**
3. Avoid duplicate test; run both builds and test suites for current branch evidence. **Done.**
4. Results: net8 build exit 0, 0 warnings; net48 exit 0, 12 warnings; focused 16/16 pass; unit suite 204 pass/2 skip; diff check exit 0. **Done.**
5. Record discrepancy/evidence and commit/push documentation. **In progress.**

## Constraints
No database/job invocation; keep task selector contract unchanged.

## Acceptance
- [ ] A real previously uncovered policy case is tested, or record the claim that cases are already covered with command evidence.
- [ ] Both TFM builds and unit tests pass; diff check clean.
- [ ] PLAN/LOOP_LOG/TODO record actual outcome, conventional commit pushed.
