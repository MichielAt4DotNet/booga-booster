## ADDED Requirements

### Requirement: Flight altitude in telemetry

Every published ride telemetry snapshot SHALL include the ride's flight altitude in metres (0 when the ride has not flown), and its lifecycle `state` SHALL be able to report `Flying`.

#### Scenario: Altitude published while flying

- **WHEN** the ride is `Flying` and a telemetry snapshot is published
- **THEN** the snapshot carries the current flight altitude and state `Flying`

#### Scenario: Altitude zero on the ground

- **WHEN** the ride has never flown and a snapshot is published
- **THEN** the flight altitude is 0
