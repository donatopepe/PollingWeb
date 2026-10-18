# PollingWeb — Iteration TODO

## Goal
Implement the first safe X2.3 worker-side contract: verify the legacy Quartz jobs carry required parameters and do not depend on HttpContext, while preserving existing scheduler gateways.

## Metric / threshold
All five logical job families build on net8/net48 and unit tests verify explicit parameter mapping/contracts without database or production runs; no domain behavior changes.

## Ordered steps
1. Confirm clean main, CodeGraph current, inspect X2.3, job classes and test project. **Done.**
2. Baseline: persisted Quartz job classes consume JobDataMap fields (`codist`, `idprocesso`, `onetime`, and order `tipoExport`); no HttpContext in job files. **Done.**
3. Create contract tests via Quartz job details for required fields/types, not executing domain jobs. **In progress.**
4. Build PollingClass net8/net48, run unit tests and diff check. **Pending.**
5. Record evidence, commit and push. **Pending.**

## Constraints
No secrets discovery, no DB login guessing, no fabricated domain schedules, preserve ordered plan.

## Acceptance
- [ ] X2.1 marked [!] with blocker and remediation path.
- [ ] PLAN and LOOP_LOG truthful; diff check passes.
- [ ] Atomic documentation commit pushed; main clean.
