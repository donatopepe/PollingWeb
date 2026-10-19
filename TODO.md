# PollingWeb — Iteration TODO

## Goal
Reuse the tested interval JobDataMap projection for supplier interval scheduling, preserving exact map keys and semantics.

## Metric / threshold
Supplier interval map uses shared policy with the legacy `codist`, `idprocesso`, `onetime` keys; zero→true, positive→false. Identity and trigger timing unchanged. Builds/tests/diff check pass; no runtime scheduler/DB.

## Ordered steps
1. Verify clean main, CodeGraph, prompt/plan and inspect supplier interval map. **Done.**
2. Baseline: other interval gateways use shared projection; supplier still populates values inline in each branch. **Done.**
3. Wire supplier interval to shared projection. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused supplier/contract 90/90; suite 230 pass/2 skip; diff check exit 0. **Done.**
5. Update records, commit/push and verify clean. **In progress.**

## Constraints
No scheduler/job/DB execution; preserve validation, identity and timing.

## Acceptance
- [ ] Supplier interval uses projection with legacy values and keys.
- [ ] Both builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO updated; push confirmed.
