# PollingWeb — Iteration TODO

## Goal
Project explicit, tested order JobDataMap construction from the pure order argument policy, preserving Quartz key names/types for daily and interval jobs.

## Metric / threshold
Policy produces exactly `codist`, `idprocesso`, `tipoExport`, `onetime` values; daily and interval tests verify existing contract, used in both gateway paths. Both builds/test suites and diff check pass.

## Ordered steps
1. Check clean main, CodeGraph, PROMPT/PLAN order and current gateway/policy tests. **Done.**
2. Baseline: tests cover normalized arguments but not the emitted JobDataMap entries shared by daily and interval gateways. **Done.**
3. Add pure JobDataMap projection and wire daily/interval gateway paths; add key/value tests. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused order tests 32/32; suite 228 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit and push. **In progress.**

## Constraints
No scheduler/job/DB execution; preserve exact map keys and string formats.

## Acceptance
- [ ] Projection emits all four legacy keys and tested values.
- [ ] Both builds/tests/diff check pass.
- [ ] Records updated; pushed conventional commit; clean tree.
