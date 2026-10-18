# PollingWeb — Iteration TODO

## Goal
Use the tested shared job-data projection in the invoice daily gateway, retaining its legacy map keys and recurring flag.

## Metric / threshold
Invoice daily adds `codist`, `idprocesso`, `onetime=false`, with no order-only `tipoExport`; job type/trigger behavior unchanged. Builds/tests/diff check pass, no scheduler/DB run.

## Ordered steps
1. Confirm clean main, CodeGraph, prompt/plan order, inspect invoice daily map. **Done.**
2. Baseline: budget and order use projection; invoice daily still writes three values directly. **Done.**
3. Reuse projection with selector omitted. **Done.**
4. PollingClass net8/net48 exit 0 (12 warnings each); focused invoice+policy 41/41; suite 228 pass/2 skip; diff check exit 0. **Done.**
5. Update evidence, commit and push. **In progress.**

## Constraints
No Quartz start/job execution/DB access; preserve invoice legacy key set.

## Acceptance
- [ ] Invoice daily map generated through policy, exact three legacy keys.
- [ ] Builds/tests/diff check pass.
- [ ] Records updated; push verified clean.
