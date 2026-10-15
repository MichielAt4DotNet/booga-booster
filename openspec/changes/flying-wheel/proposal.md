## Why

BoogaBooster is a showpiece and a teaching demo; a ride that can literally take off ("yeet") is a memorable, fun capability that also exercises the full stack — a new guarded lifecycle state, deterministic physics, telemetry, and a 3D camera behaviour.

## What Changes

- Add a new lifecycle state **Flying** to the ride state machine, reachable only from **Started** through an operator-triggered transition (the "yeet").
- Guard the transition: allowed only when the ride is `Started` (never `Stopping`/`Emergency Stop`), central mill power is **100 %** and hub power is **100 %**.
- While `Flying`, the whole rig (the base) comes loose from the ground and climbs; the climb is simulated deterministically in the digital twin and published as a flight altitude in telemetry.
- Add a **"Yeet"** button to the dashboard's *Lifecycle transitions* group; it is enabled only when the backend offers the `Flying` transition.
- 3D visualization: the base detaches from the ground and rises with the reported altitude; once it is a little way up (threshold altitude) the camera starts following the ride.
- Emergency stop and controlled stop remain available while flying.

## Capabilities

### New Capabilities
- `ride-flight`: the Flying lifecycle state, its guard, the deterministic take-off/climb simulation, and the flight telemetry.

### Modified Capabilities
- `ride-state-machine`: new `Flying` state and `Started → Flying` transition; emergency stop reachable from `Flying`.
- `ride-operation-controls`: the lifecycle transitions group gains the "Yeet" button with its availability rules.
- `ride-telemetry-streaming`: telemetry carries the flight altitude.
- `ride-dashboard-layout`: the 3D view lifts the base off the ground and the camera follows the ride once airborne.

## Impact

- **Backend** (`DigitalTwin`): `RideState` enum (Abstractions), `Ride` state machine, `RideParameters` (all flight constants), telemetry DTO, `RequestRideStateTransition` handler/validation, tests.
- **Frontend**: `ride.models.ts` (state list, indices, labels, altitude field), `status-summary`, `ride-visualization` (base lift + follow camera), specs incl. a11y.
- **Docs**: `docs/` gains a short flight-physics derivation; `docs/appendix-parameters.md` the new constants.
- No new dependencies; no integration events.
