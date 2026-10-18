# PollingWeb — Iteration TODO

## Goal
Reuse the tested order JobDataMap projection for the budget gateway's daily job data (one nonduplicative gateway contract step).

## Metric / threshold
Budget daily gateway constructs its legacy three keys via the tested pure policy, preserving its job type, key names, values and string encoding. Both TFMs/tests/diff check pass; no Quartz/DB runtime.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan order, inspect map writes and existing projection. **Done.**
2. Baseline: order uses shared map projection, budget daily writes same contract keys separately. **Done.**
3. Wire budget daily gateway to shared projection; existing tests already pin exact projection keys/values, avoid duplicate coverage. **Done.**
4. PollingClass net8/net48 exit 0 (5 warnings each); focused budget+order policy 40/40; suite 228 pass/2 skip; diff check exit 0. **Done.**
5. Record evidence, commit and push. **In progress.**

## Constraints
Preserve existing Quartz job identity and values; do not start scheduler/job or access DB.

## Acceptance
- [ ] Budget daily projection uses tested keys and values.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; pushed; clean tree confirmed.
