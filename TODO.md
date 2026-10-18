# PollingWeb — Iteration TODO

## Goal
Avoid an unsupported claim in X2.3 tests: replace source-text inspection of job classes with verifiable metadata-only contract checks, if practical without adding unrelated dependencies.

## Metric / threshold
Test project remains deterministic and works from normal test output without locating repository source files; retain Quartz type/signature/attribute checks. Build/test pass; no runtime jobs.

## Ordered steps
1. Verify clean main, CodeGraph, prompt, plan and current source-reading test. **Done.**
2. Baseline: contract test walks parent dirs until finding `PollingWeb.sln` then reads C# source, coupling tests to checkout layout and asserting text rather than execution behavior. **Done.**
3. Remove brittle source-text asserts and keep metadata contracts. **Done.**
4. PollingClass net8 exit 0/0 warning, net48 exit 0/12 warning; focused WorkerJobContractTests 38/38 pass; unit suite 210 pass/2 skip; `git diff --check` exit 0. **Done.**
5. Update evidence, commit and push. **In progress.**

## Constraints
No domain/job/DB behavior changes, no new dependencies, no source path assumptions.

## Acceptance
- [ ] Tests no longer require locating solution/source files.
- [ ] Builds/tests/diff check pass.
- [ ] PLAN/LOOP_LOG/TODO accurate and pushed commits.
