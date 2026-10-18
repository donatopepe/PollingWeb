# PollingWeb — Iteration TODO

## Goal
Unify existing interval gateways on the pure X2.3 policy for `onetime`, verifying zero/positive behavior remains unchanged.

## Metric / threshold
All five interval gateway implementations use `IntervalOnetimeValue`; tests prove zero→true, positive→false, negative throws; PollingClass net8/net48 builds and unit suite pass. No scheduler/DB invocation.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan; inspect interval gateway implementations and current policy tests. **Done.**
2. Baseline: gateways duplicate inline string true/false branches; one-time semantics vary only by interval == 0. **Done.**
3. Replace those duplicated values with shared policy and add invalid-negative assertion. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused WorkerJobContractTests 32/32 pass; unit suite 199 pass/2 skip; diff check exit 0. **Done.**
5. Update plan/log, commit and push. **In progress.**

## Constraints
Keep identities, trigger timing, and scheduler side effects unchanged; do not start a scheduler or connect to SQL.

## Acceptance
- [ ] Interval gateway values use validated shared policy without changing timing behavior.
- [ ] Both target builds/tests/diff check pass.
- [ ] Evidence and commits published; tree clean.
