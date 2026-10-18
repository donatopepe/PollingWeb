# PollingWeb — Iteration TODO

## Goal
Strengthen X2.3 job contracts by asserting that recurring job flag inputs are present in every Quartz job's declared persisted data map.

## Metric / threshold
All six job classes are Quartz `IJob` types; shared recurring map includes `onetime=false`; tests verify map value and conversion. Build PollingClass net8/net48 and unit suite; no database or job execution.

## Ordered steps
1. Confirm clean main/CodeGraph, prompt, plan and job classes/gateway contracts. **Done.**
2. Baseline: Quartz jobs consume keys at Execute; recurring flag policy exists but tests do not exercise a JobDataMap structure. **Done.**
3. Add a Quartz `JobDataMap` contract test for the shared recurring flag. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); focused job contracts 28/28 pass; unit suite 195 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG, commit and push. **In progress.**

## Constraints
No real DB, scheduler startup, secrets, or domain job invocation.

## Acceptance
- [ ] Contract test checks `JobDataMap` recurring key.
- [ ] Builds/tests/diff check pass.
- [ ] Record actual evidence and push task commits.
