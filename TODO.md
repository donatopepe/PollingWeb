# PollingWeb — Iteration TODO

## Goal
Continue T3 with one minimal synthetic SQL integration assertion, leaving SQL/domain behavior unchanged.

## Metric / threshold
1/1 uncovered behavior verified; IntegrationTests project build exits 0; focused test passes against both SQL function copies; `git diff --check` exits 0.

## Ordered steps
1. Verify clean `main`, current index, T3 plan and test/log baseline. **Done.**
2. Select gap: empty element `<></>`; SQL replaces the empty tag text with a space, then replaces its closing tag with another space. Expected exact result: two spaces. **Done.**
3. Add synthetic input/assertion to shared test. **In progress.**
4. Build → exit 0; focused test first exposed one extra replacement space, corrected expected output, rerun → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Record actual result in PLAN and LOOP_LOG; atomic T3 commit and push. **In progress.**

## Constraints
No SQL changes or real records; credentials only in process environment. Do not download images, claim other OS/full-solution verification, or leak secret-bearing patterns.

## Acceptance
- [x] Both deployed `StripHTML` copies return exactly two spaces for empty HTML element `<></>`.
- [x] Build, focused SQL test, and whitespace check pass.
- [x] PLAN §8 and LOOP_LOG show observed outcomes.
- [ ] T3 commit pushed on main; tree clean.
