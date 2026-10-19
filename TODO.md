# PollingWeb — Iteration TODO

## Goal
Use the tested shared `ParseOneTime` conversion policy in `JobSQL01` and add explicit true/false dispatch-contract coverage.

## Metric / threshold
`JobSQL01` parses Quartz `onetime` via shared policy; true/false behavior pinned, legacy calls/logging/exception handling unchanged. Both TFM builds, focused/full tests and diff check pass. No job runtime.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan order, inspect SQL01 job and dispatch policy/tests. **Done.**
2. Baseline: `JobBudget` uses shared parse policy; `JobSQL01` still performs inline `Convert.ToBoolean`. **Done.**
3. Apply same pure parser to SQL01; existing true/false tests cover shared parser. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused WorkerJobContract+SQL01 60/60; suite 248 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push and confirm clean. **In progress.**

## Constraints
Do not execute SQL01, access database or alter log/message behavior.

## Acceptance
- [ ] SQL01 uses shared conversion policy and true/false tested.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; pushed cleanly.
