# PollingWeb — Iteration TODO

## Goal
Complete safe X2.1 worker bootstrap by hosting the persistent Quartz scheduler and shutting it down cleanly. Job registration and source scanning remain X2.3 scope.

## Metric / threshold
Worker builds with zero errors and attempts scheduler startup through configured connection; shutdown awaits Quartz. Missing or unavailable SQL configuration yields a visible startup failure, not a falsely-ready worker.

## Ordered steps
1. Confirm clean main, inspect X2.1/X2.3, factory behavior and logs. **Done.**
2. Baseline: worker skeleton idles without scheduler; factory requires Quartz SQL configuration. **Done.**
3. Start and stop Quartz from Worker lifecycle, leaving job registration to X2.3. **In progress.**
4. Build Worker and test configured startup; verify graceful shutdown. **Pending.**
5. Record actual outcomes, commit and push. **Pending.**

## Constraints
Do not fabricate source scanning or schedule jobs before X2.3; credentials only in process environment; no image pulls or claims for unavailable platforms.

## Acceptance
- [ ] Worker registers Quartz lifecycle and awaits clean scheduler shutdown.
- [ ] Project build passes; startup behavior recorded with actual configured-DB evidence.
- [ ] PLAN §8 and LOOP_LOG document real outcomes.
- [ ] Atomic X2.1 conventional commit pushed and main clean.
