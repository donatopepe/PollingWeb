# PollingWeb — Iteration TODO

## Goal
Strengthen X2.3 order job contract coverage: ensure the order send job's required `tipoExport` JobDataMap value is present and routing policy is tested for send/receipt modes.

## Metric / threshold
Test `OrdiniJobDataPolicy.TipoExport` returns the supplied export type for send, empty for receipt, and preserves null normalization; PollingClass net8/net48 build and unit suite pass. No DB/scheduler invocation.

## Ordered steps
1. Verify clean main, CodeGraph, plan and order job/gateway source. **Done.**
2. Baseline: send Execute reads `tipoExport`; gateway supplies it through policy, but policy has no direct unit tests. **Done.**
3. Add focused pure policy tests for send/receipt/null cases. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); order policy tests 16/16 pass; unit suite 204 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/log; commit and push. **In progress.**

## Constraints
No schedule/domain mutation, DB, credentials, or scheduler startup.

## Acceptance
- [ ] Export-type mapping tests cover send/receipt and null normalization.
- [ ] Both builds/tests/diff check pass.
- [ ] Evidence recorded and task commit pushed; tree clean.
