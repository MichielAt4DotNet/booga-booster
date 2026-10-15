## Context

The ride lifecycle is a guarded state machine in `DigitalTwin/Domain/Ride.cs` (states in `RideState`, legal operator transitions each with a guard). The backend exposes `availableTransitions` in telemetry and the Angular `status-summary` renders one button per lifecycle state from that list; the 3D view (`ride-visualization.ts`, Three.js + `OrbitControls`) draws a static base on the ground. Motor power is set through `SetMainEnginePower` / `SetHubEnginePower` (0–100 %).

## Goals / Non-Goals

**Goals:**
- A "Yeet" button that is enabled exactly when `Started`, not stopping, mill power = 100 %, hub power = 100 %.
- Deterministic take-off and climb; base visibly leaves the ground; camera follows once a little way up.

**Non-Goals:**
- Flight dynamics beyond altitude (no horizontal travel, landing, or crash).
- Passenger-safety interaction (covered by the separate `passenger-safety` change).

## Decisions

- **D1 — Flying is a real lifecycle state, not a frontend flag.** The backend owns legality: `Started → Flying` with a guard `MillPower == 100 && HubPower == 100`. The "not stopping" condition holds structurally because the transition originates only from `Started`. The frontend merely renders a button for `flying` when it appears in `availableTransitions`, consistent with the existing "click only emits" contract. *Alternative:* frontend-only toggle gated on telemetry — rejected, it would bypass the state machine and violate determinism/authority.
- **D2 — Altitude is simulated in the domain and published in telemetry.** A new `FlightAltitudeMeters` on the ride, advanced in `Advance(dt)` while `Flying` with constant-acceleration lift-off (`a = FlightLiftAcceleration`, capped at `FlightClimbSpeedMax`), integrated on the fixed 1/120 s step. Pure function of state → deterministic. All constants (`FlightLiftAcceleration`, `FlightClimbSpeedMax`, `FlightCameraFollowAltitude`) live in `RideParameters`, derived in `docs/`.
- **D3 — Camera follow threshold is a shared constant.** The frontend mirrors `FlightCameraFollowAltitude` (same "must stay in sync" pattern as `MILL_MAX_POWER_WATTS`). Below the threshold the orbit camera stays put while the base rises; at/above it the orbit target tracks the base each frame and the camera keeps its relative offset, preserving user orbiting.
- **D4 — Stopping while flying.** `Flying → Stopping` and `→ Emergency Stop` are allowed; altitude then holds (no landing in this change) and the ride settles via existing rest logic. The ride cannot return to `Started`.
- **D5 — Motor power is clamped while flying.** Power setters stay accepted but flight continues only while in `Flying`; leaving the state ends lift.
- **Handlers** keep `ExecuteAsync` and ADR-0009 instrumentation unchanged; the existing `RequestRideStateTransition` command is reused (new state value only). The `RideState` JSON/ index mapping gains `Flying` appended at the end to keep existing indices stable.

## Risks / Trade-offs

- Appending `Flying` after `EmergencyStop` (index 7) keeps wire compatibility but means ordering in UI lists is explicit → the frontend uses an explicit ordered list rather than index order.
- Mirrored constant can drift → covered by a model spec test and a note in `docs/appendix-parameters.md`.
- Unbounded climb → capped by max climb speed only; acceptable for a demo, flagged as non-goal for landing.
