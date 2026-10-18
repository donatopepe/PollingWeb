# PollingWeb — Iteration TODO

## Goal
Continue the first incomplete ordered plan task, T3, with one small synthetic integration-test improvement and verifiable evidence, preserving SQL/domain behavior.

## Metric / threshold
Iteration pass: 1/1 behaviorally grounded test addition verified by a successful touched-project build, focused SQL test against both deployed function copies, and clean `git diff --check`.

## Ordered steps
1. Confirm main is clean/current; inspect T3 coverage and prior loop entries. **Done.**
2. Select one uncovered behavior and state expected result before edit. **Done.**
3. Add one synthetic case only to the existing test; leave SQL definitions untouched. Chosen gap: adjacent HTML elements (prior markup case only had one `<p>`); expected output follows the implementation's replacement of each element with a space. **Done.**
4. Build IntegrationTests → exit 0, 12 existing warnings; focused SQL test → 1/1 pass; `git diff --check` → exit 0. **Done.**
5. Record observed evidence in PLAN §8 and LOOP_LOG; commit atomically and push `origin main`. **In progress.**

## Dependencies and constraints
- Existing SQL Server container/schema; do not pull images.
- Credentials only in process environment, never source or output.
- No macOS/Windows claim; full-solution build is known limited by the Website project/MSBuild.
- No production data; use only synthetic input and rollback-safe test pattern.

## Acceptance criteria
- [x] Test checks adjacent two-element markup in both `dbo.StripHTML` and `eProcurement.dbo.StripHTML`; expected output derives from each tag being replaced by one space.
- [x] IntegrationTests build and focused SQL test pass; `git diff --check` passes.
- [x] PLAN §8 and LOOP_LOG record only actual results.
- [ ] Conventional T3 commit pushed to main; working tree clean.
