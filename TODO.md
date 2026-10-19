# PollingWeb — Iteration TODO

## Goal
Ensure the new immutable task mapping returns a fresh result and cannot retain mutable state across separate policy lookups.

## Metric / threshold
All type 1–7 lookups yield non-null mappings with stable existing values; repeated calls are distinct immutable instances; existing invalid-type behavior unchanged. Builds/tests/diff check pass.

## Ordered steps
1. Confirm clean main, CodeGraph, task order and mapping implementation/tests. **Done.**
2. Baseline: mapping immutable by accessors but identity/repeated lookup isolation is not explicitly tested. **Done.**
3. Add targeted immutability/fresh-instance test for supported task types 1–7. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); WorkerJobContractTests 47/47; suite 244 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push, confirm clean. **In progress.**

## Constraints
No map-value or routing changes; no DB/job execution.

## Acceptance
- [ ] Repeated lookups return independent immutable mapping results for all 7 types.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO reflect actual evidence; pushed cleanly.
