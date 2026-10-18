# PollingWeb — Iteration TODO

## Goal
Continue T3 with one synthetic SQL integration assertion without changing SQL/domain behavior.

## Metric / threshold
1/1 uncovered behavior verified; project build exit 0, focused SQL test passes against both function copies, diff check exit 0.

## Ordered steps
1. Confirm clean main, read current T3 log/test/function source. **Done.**
2. Baseline gap: tag with self-closing syntax (`<br/>`) not tested. The implementation removes any substring from `<` through the next `>` and replaces it with one space, so expected result for `left<br/>right` is `left right`. **Done.**
3. Add only a synthetic assertion to the shared test. **Done.**
4. Build → exit 0, 12 existing warnings; filtered SQL test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Record actual evidence, commit atomically with T3 ID, push. **Pending.**

## Constraints
Do not modify SQL definitions; synthetic input only. DB credential stays process environment. No image pulls; no claims for unavailable operating systems or full solution build.

## Acceptance
- [x] Both `dbo.StripHTML` and `eProcurement.dbo.StripHTML` turn `left<br/>right` into `left right`.
- [x] IntegrationTests build, focused test, and diff check succeed.
- [x] PLAN §8 and LOOP_LOG record actual evidence.
- [ ] Conventional T3 commit pushed; worktree clean.
