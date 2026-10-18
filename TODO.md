# PollingWeb — Iteration TODO

## Goal
Continue T3 with one minimal synthetic database integration assertion, preserving stored-procedure/function behavior.

## Metric / threshold
Pass 1/1: new assertion grounded in SQL implementation; IntegrationTests project build succeeds; focused SQL test passes on both deployed function copies; `git diff --check` succeeds.

## Ordered steps
1. Confirm clean main and read task/log/test source. **Done.**
2. Baseline gap: existing `StripHTML` case covers CRLF but not preservation of a horizontal tab in plain text. Expected: input `left\tright` retains the tab, since implementation replaces selected punctuation and does not rewrite tabs. **Done.**
3. Add just this synthetic result to the shared test for both function copies. **Done.**
4. Build → exit 0 (12 existing warnings); focused SQL test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Update PLAN §8 and LOOP_LOG with observed results, commit, push, confirm clean tree. **In progress.**

## Constraints
No SQL definition/domain changes, no real data, no secret in tracked files/output, no image pulls. Linux only; do not claim macOS/Windows or full-solution verification.

## Acceptance
- [x] Test asserts tab preservation in both `dbo.StripHTML` and `eProcurement.dbo.StripHTML`.
- [x] Build + focused SQL test + diff check pass.
- [x] PLAN and loop log record actual outcomes.
- [ ] Conventional T3 commit is pushed; tree clean.
