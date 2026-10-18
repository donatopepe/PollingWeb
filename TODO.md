# PollingWeb — Iteration TODO

## Goal
Continue T3 with one minimal synthetic SQL integration assertion, without changing function/procedure definitions.

## Metric / threshold
One distinct behavior checked in both deployed `StripHTML` copies; IntegrationTests project build exit 0; focused SQL test 1/1 pass; `git diff --check` exit 0.

## Ordered steps
1. Confirm clean current main, read T3 specification/log, and inspect test/function. **Done.**
2. Baseline gap: apostrophe replacement was exercised in a combined phrase but there was no exact test for a lone apostrophe. Expected: a single ASCII apostrophe becomes one space. **Done.**
3. Add one synthetic expression/assertion to existing shared function query. **Done.**
4. Build → exit 0, 12 existing warnings; focused test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Update PLAN §8 and LOOP_LOG with actual evidence; commit and push. **In progress.**

## Constraints
No SQL implementation changes or real data. DB credential via process environment only. No image pulls or claims of macOS/Windows/full-solution verification.

## Acceptance
- [x] Both `dbo.StripHTML` and `eProcurement.dbo.StripHTML` return one space for one ASCII apostrophe.
- [ ] IntegrationTests build and focused SQL test pass; diff check passes.
- [x] Plan and loop log updated with observed results.
- [ ] Atomic conventional T3 commit pushed; main worktree clean.
