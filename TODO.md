# PollingWeb — Iteration TODO

## Goal
Continue T3 with one focused synthetic SQL integration case; do not alter SQL/domain behavior.

## Metric / threshold
Verify one uncovered parser edge against both deployed `StripHTML` functions; project build succeeds, focused SQL test passes, diff check succeeds.

## Ordered steps
1. Confirm clean main and inspect current T3 evidence, test and SQL implementation. **Done.**
2. Gap: `>` inside a quoted HTML attribute is untested. The SQL implementation ends a tag at the first `>` regardless of quotes; expected residual output for `<b title='a > b'>x</b>` is grounded in that literal behavior. **Done.**
3. Add one synthetic expression and exact assertion to shared test. **Done.**
4. Build → exit 0 (12 existing warnings); focused SQL test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Record results in PLAN §8 and LOOP_LOG, commit/push. **In progress.**

## Constraints
Synthetic input only, no SQL changes, no credentials in files/output, no image pulls, no cross-OS/full-solution claim.

## Acceptance
- [x] Both function copies return `  b >x ` for `<b title='a > b'>x</b>` under current first-`>` parsing.
- [ ] Build, focused test, and `git diff --check` pass.
- [x] PLAN and LOOP_LOG record actual evidence.
- [ ] Atomic T3 commit pushed; clean main.
