## 1. Backend domain (DigitalTwin)

- [ ] 1.1 Add `Flying` (appended, index 7) to `RideState` and update the index/name mapping and any exhaustive switches
- [ ] 1.2 Add flight constants (`FlightLiftAcceleration`, `FlightClimbSpeedMax`, `FlightCameraFollowAltitude`) to `RideParameters`; document the derivation in `docs/` and `docs/appendix-parameters.md`
- [ ] 1.3 Add the `Started → Flying` transition guarded by mill power = 100 % and hub power = 100 %; allow `Flying → Stopping` and `Flying → Emergency Stop`
- [ ] 1.4 Add deterministic `FlightAltitudeMeters` integration in `Ride.Advance` while `Flying`
- [ ] 1.5 Treat `Flying` as running/constraints-locked and make the stop and settle-at-rest logic work from `Flying`

## 2. Backend API and telemetry

- [ ] 2.1 Add the flight altitude to `RideTelemetry` and its DTO/SSE payload
- [ ] 2.2 Verify `RequestRideStateTransition` accepts `Flying` and returns 400 on a rejected guard

## 3. Backend tests

- [ ] 3.1 State-machine tests: Yeet accepted at 100/100, rejected at <100 mill, <100 hub, from `Stopping`/`Safe`; available transitions
- [ ] 3.2 Flight tests: altitude rises, is capped by max climb speed, deterministic, zero when never flown, stops on emergency stop
- [ ] 3.3 Keep `DigitalTwin` line coverage ≥ 80 %

## 4. Frontend

- [ ] 4.1 Add `flying` to the `RideState` model, index mapping, label ("Yeet") and ordered lifecycle list; add the altitude field and mirrored follow-threshold constant
- [ ] 4.2 Render the "Yeet" button in `status-summary`, enabled from the server's available transitions
- [ ] 4.3 `ride-visualization`: lift the base/rig by the altitude, keep the ground fixed
- [ ] 4.4 `ride-visualization`: follow the ride with the camera once altitude ≥ threshold, preserving user orbit
- [ ] 4.5 Wire the altitude from telemetry into the visualization

## 5. Frontend tests and accessibility

- [ ] 5.1 Vitest specs: model mapping, Yeet enabled/disabled/click, altitude lift and follow behaviour
- [ ] 5.2 Extend the dashboard a11y spec so the Yeet button passes AXE / WCAG AA

## 6. Wrap-up

- [ ] 6.1 `dotnet build` and `dotnet test` the solution; `npm test` in the Angular app
- [ ] 6.2 Run `node .claude/skills/ai-tooling-sync/scripts/sync-ai-tooling.mjs` if tooling files changed
