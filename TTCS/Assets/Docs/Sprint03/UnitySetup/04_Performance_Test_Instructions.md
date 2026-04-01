# Performance Test Instructions - Sprint 3 (Gameplay Baseline)

## Purpose
Measure baseline responsiveness for core loop:
Main Menu -> Level Select -> Combat -> Reward -> Save.

## Metrics to Capture
- Scene transition time (target < 2s on dev machine)
- Combat completion to reward apply latency (target < 200ms logic-side)
- Save write latency (target < 100ms for small save payload)
- No frame hitch > 50ms during reward apply and save

## Procedure
1. Open Unity Profiler (`Window > Analysis > Profiler`).
2. Enable CPU and Memory modules.
3. Run 10 consecutive level runs.
4. Record:
- Transition spikes
- GC allocation spikes
- Save call duration

## Pass Criteria
- No growing memory leak trend across 10 runs.
- Save/Reward path remains stable (no increasing latency trend).
- No fatal exceptions in console.

## If Fails
1. Inspect high-allocation methods in Profiler hierarchy.
2. Reduce temporary allocations in reward/save application path.
3. Re-run same 10-run benchmark.
