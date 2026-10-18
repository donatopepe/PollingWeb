# PollingWeb — Iteration TODO

## Goal
Continue T3 with one narrowly-scoped synthetic SQL integration assertion, without changing domain behavior.

## Metric / threshold
One uncovered SQL-function behavior verified on both deployed copies; touched project build exit 0, filtered SQL test 1/1 pass, `git diff --check` exit 0.

## Ordered steps
1. Verify clean main and inspect T3 plan, recent log, test and SQL function. **Done.**
2. Baseline gap: backtick replacement appears only in a combined punctuation case; no plain text assertion isolates it. Expected `a b` for `a` + backtick + `b`. **Done.**
3. Add one synthetic query term and exact assertion to existing shared test. **Done.**
4. Initial build found a missing C# concatenation operator; fixed. Build exit 0; focused SQL test 1/1 pass; diff check exit 0. **Done.**
5. Record actual evidence in plan and log; commit/push atomically. **In progress.**

## Constraints
No SQL changes or real data. Secret only in process environment, never output/files. No image downloads, no full-solution or non-Linux claims.

## Acceptance
- [x] Both function copies replace a lone backtick between letters with one space.
- [ ] Build, focused SQL test, and whitespace check pass.
- [x] PLAN §8 and LOOP_LOG record real results.
- [ ] Conventional T3 commit pushed, clean main.
