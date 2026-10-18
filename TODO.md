# PollingWeb — Iteration TODO

## Goal
Test Quartz `JobDataMap` persistence semantics for the mutable one-time flag on persisted jobs, a relevant X2.3 contract risk.

## Metric / threshold
A Quartz `JobDataMap` with `PersistJobDataAfterExecution` proves storing a changed string boolean returns the changed value after a simulated job-data update; focused/full tests, PollingClass net8/net48 builds pass. No scheduler/database.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan, job attributes and current tests. **Done.**
2. Baseline: job classes carry `PersistJobDataAfterExecution`; current contract tests don't check whether mutating map values can persist without scheduler. **Done.**
3. Add an offline `JobDataMap` persistence-data contract test, documenting no real scheduler run. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused persistence-map tests 6/6 pass; unit suite 210 pass/2 skip; diff check exit 0. **Done.**
5. Update records; commit and push. **In progress.**

## Constraints
Do not start scheduler/DB, trigger jobs, inspect credentials, or claim runtime persistence validated.

## Acceptance
- [ ] Offline map mutation/readback test exists; no scheduler runtime claim.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG accurate; conventional commit pushed and tree clean.
