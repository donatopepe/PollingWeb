# PollingWeb — Iteration TODO

## Goal
Verify the next X2.3 detail: whether the invoice-import Quartz adapter forwards source/interval configuration and preserves process identity without inventing behavior.

## Metric / threshold
Baseline source inspection identifies exact existing persisted job fields and trigger properties; add a single synthetic policy/contract test only if a genuine gap is demonstrated, otherwise document evidence. No DB access or production schedules.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan and inspect Invoice job + adapter. **Done.**
2. Baseline: daily invoice adapter omitted `onetime`, but `JobImportFatture.Execute` calls `Convert.ToBoolean(GetString("onetime"))`; null would throw and be swallowed by handler. **Done.**
3. Add explicit false recurring flag to daily invoice job data with a pure policy and test. **Done.**
4. PollingClass net8/net48 builds exit 0 (12 warnings each); invoice tests 9/9 pass; unit suite 185 pass/2 skip; diff check exit 0. **Done.**
5. Update log/plan and commit/push. **In progress.**

## Constraints
No secrets discovery, no DB login guessing, no fabricated domain schedules, preserve ordered plan.

## Acceptance
- [ ] One X2.3 gap addressed or documented as already present with executable evidence.
- [ ] Both PollingClass TFMs build; tests pass; diff check passes.
- [ ] PLAN and LOOP_LOG document actual outcomes.
- [ ] Conventional X2.3 commit pushed; main clean.
