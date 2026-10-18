# PollingWeb — Iteration TODO

## Goal
Codify the legacy TaskService task-type routing in a pure X2.3 policy with exhaustive synthetic unit coverage, preserving the WebForms switch behavior.

## Metric / threshold
Types 1–7 map to the existing job classes/families exactly as `TaskService.aspx.cs`; unknown types reject explicitly. PollingClass builds both TFMs; focused tests pass; no database/domain jobs execute.

## Ordered steps
1. Confirm main status, prompt, plan, policy/job types and WebForms routing. **Done.**
2. Baseline: `TaskService.aspx.cs:RischedulaTask` routes types 1–7 to supplier, ODA, receipt, budget, SQL01, RDA, invoices respectively. **Done.**
3. Add pure mapping policy and table-driven assertions, keeping scheduler side effects out. **Done.**
4. Build PollingClass net8/net48; run focused and full unit suite; diff check. **In progress.**
5. Record evidence, commit and push. **Pending.**

## Constraints
No secrets discovery, no DB login guessing, no fabricated domain schedules, preserve ordered plan.

## Acceptance
- [ ] X2.1 marked [!] with blocker and remediation path.
- [ ] PLAN and LOOP_LOG truthful; diff check passes.
- [ ] Atomic documentation commit pushed; main clean.
