# PollingWeb — Iteration TODO

## Goal
Pin shared `ParseOneTime` edge semantics for null and malformed JobDataMap strings, ensuring legacy `Convert.ToBoolean` behavior remains explicit.

## Metric / threshold
Null maps to false; malformed non-null throws `FormatException`; existing `true/false` coverage remains. Pure unit tests only; both PollingClass TFMs, focused/full suite and diff check pass.

## Ordered steps
1. Verify clean main, CodeGraph, PROMPT/PLAN order and shared parser/call sites. **Done.**
2. Baseline: `ParseOneTime` delegates to `Convert.ToBoolean`; tests cover only valid strings. **Done.**
3. Add null and malformed-value tests without changing production behavior. **Done.**
4. PollingClass net8/net48 exit 0, zero warnings; WorkerJobContractTests 50/50; full suite 249 pass/2 skip; diff check exit 0. **Done.**
5. Update PLAN/LOOP_LOG/TODO; commit/push; verify clean. **In progress.**

## Constraints
No job or DB execution; no change to parser semantics.

## Acceptance
- [ ] Null and malformed parser behavior explicitly tested.
- [ ] Builds/tests/diff check pass.
- [ ] Actual evidence recorded and pushed cleanly.
