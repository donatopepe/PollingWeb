# PollingWeb — Iteration TODO

## Goal
Add an offline regression test for the order daily gateway's override policy, whose same-trigger replacement behavior differs from other gateways.

## Metric / threshold
A pure policy encodes and tests that order daily override unschedules existing trigger, while no-override preserves duplicate protection; no scheduler or DB is started. Build/test and diff check pass.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan, order gateway implementation and policy tests. **Done.**
2. Baseline: order daily gateway conditionally unschedules trigger on override; existing policy tests don't assert this branch. **Done.**
3. Add a pure policy function for the override decision and table-driven tests; wire only the existing condition. **Done.**
4. PollingClass net8/net48 exit 0, 12 warnings each; focused order tests 18/18; unit suite 212 pass/2 skip; diff check exit 0. **Done.**
5. Update records; commit and push. **In progress.**

## Constraints
Preserve current behavior exactly; do not create scheduler/DB connections or schedule jobs.

## Acceptance
- [ ] Policy and gateway agree for both override values.
- [ ] Builds/tests/diff check pass.
- [ ] Actual evidence recorded, task commits pushed, tree clean.
