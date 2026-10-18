# PollingWeb — Iteration TODO

## Goal
Continue T3 with one synthetic SQL integration assertion without changing SQL/domain behavior.

## Metric / threshold
1/1 uncovered behavior verified; project build exit 0, focused SQL test passes against both function copies, diff check exit 0.

## Ordered steps
1. Confirm clean main, read current T3 log/test/function source. **Done.**
2. Baseline gap: numeric-only HTML entity (`&#65;`) not tested. `StripHTML` removes markup tags, not character entities, so expected text remains unchanged. **Done.**
3. Add only a synthetic assertion to the shared test. **Done.**
4. Build → exit 0, 12 existing warnings; filtered SQL test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Record actual evidence, commit atomically with T3 ID, push. **Pending.**

## Constraints
Do not modify SQL definitions; synthetic input only. DB credential stays process environment. No image pulls; no claims for unavailable operating systems or full solution build.

## Acceptance
- [x] Both `dbo.StripHTML` and `eProcurement.dbo.StripHTML` preserve `&#65;` unchanged.
- [ ] IntegrationTests build, focused test, and diff check succeed.
- [ ] PLAN §8 and LOOP_LOG record actual evidence.
- [ ] Conventional T3 commit pushed; worktree clean.
