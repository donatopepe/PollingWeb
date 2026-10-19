# PollingWeb — Iteration TODO

## Goal
Complete reuse of the pure interval JobDataMap projection in the order gateway's daily path by extracting its recurring data flag to the same policy.

## Metric / threshold
Daily order map uses shared projection, preserves all four keys and exact values (`onetime=false`); daily job selection and scheduling behavior untouched. Both TFM builds, focused/full tests, diff check pass; no Quartz/DB runtime.

## Ordered steps
1. Confirm clean main, CodeGraph, PROMPT/PLAN order; inspect order daily/interval construction. **Done.**
2. Baseline: order interval already projects map; daily still adds keys inline and separately derives false. **Done.**
3. Use shared `JobData` projection for daily order map and test exact four keys/values. **Done.**
4. PollingClass net8/net48 exit 0 (5 warnings each); focused order policy tests 35/35; suite 231 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit/push, verify clean. **In progress.**

## Constraints
No job/scheduler/DB execution; retain send/receipt mapping and order identities/trigger policy.

## Acceptance
- [ ] Daily order map projected by tested pure policy, exact legacy map.
- [ ] Both builds/tests/diff check pass.
- [ ] Records updated, commit pushed, clean tree confirmed.
