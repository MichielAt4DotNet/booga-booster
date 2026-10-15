## ADDED Requirements

### Requirement: Yeet transition guard

The ride SHALL allow the operator transition **Started → Flying** only when the ride is `Started` (and therefore not `Stopping`), the central mill power is 100 % and the hub power is 100 %. A request that does not satisfy every condition SHALL be rejected without changing state.

#### Scenario: Yeet accepted at full power

- **WHEN** the ride is `Started`, mill power is 100 % and hub power is 100 % and a transition to `Flying` is requested
- **THEN** the ride's state becomes `Flying`

#### Scenario: Yeet rejected below full mill power

- **WHEN** the ride is `Started`, mill power is 99 % and hub power is 100 % and a transition to `Flying` is requested
- **THEN** the request is rejected with a domain validation error
- **AND** the state remains `Started`

#### Scenario: Yeet rejected below full hub power

- **WHEN** the ride is `Started`, mill power is 100 % and hub power is 80 % and a transition to `Flying` is requested
- **THEN** the request is rejected and the state remains `Started`

#### Scenario: Yeet rejected while stopping

- **WHEN** the ride is `Stopping` and a transition to `Flying` is requested
- **THEN** the request is rejected and the state remains `Stopping`

### Requirement: Take-off and climb

While `Flying`, the ride's base SHALL come loose from the ground and climb. The flight altitude SHALL start at 0 m, accelerate at a constant lift acceleration up to a maximum climb speed, and be advanced deterministically on the fixed physics timestep. All flight constants SHALL be defined in `RideParameters`.

#### Scenario: Altitude rises while flying

- **WHEN** the ride has been `Flying` for several simulated seconds
- **THEN** its flight altitude is greater than 0 m and monotonically non-decreasing

#### Scenario: Deterministic climb

- **WHEN** two rides enter `Flying` and are advanced by the same sequence of timesteps
- **THEN** their flight altitudes are identical

#### Scenario: Altitude is zero when not flying

- **WHEN** the ride has never entered `Flying`
- **THEN** its flight altitude is 0 m

### Requirement: Stopping a flying ride

The operator SHALL be able to request `Stopping` or `Emergency Stop` from `Flying`. Flight lift SHALL cease once the ride leaves `Flying`, and the ride SHALL NOT return to `Started`.

#### Scenario: Emergency stop while flying

- **WHEN** the ride is `Flying` and `Emergency Stop` is requested
- **THEN** the state becomes `Emergency Stop` and the altitude stops increasing

### Requirement: Camera follows the airborne ride

The 3D visualization SHALL render the base at the reported flight altitude, and once the altitude reaches the follow threshold the camera SHALL track the ride so it stays in view. Below the threshold the camera SHALL NOT move on its own.

#### Scenario: Base lifts off

- **WHEN** telemetry reports a flight altitude of 2 m
- **THEN** the base is drawn 2 m above the ground

#### Scenario: Camera starts following

- **WHEN** the reported altitude first reaches the follow threshold
- **THEN** the camera's orbit target begins tracking the ride each frame

#### Scenario: Camera idle before threshold

- **WHEN** the reported altitude is below the follow threshold
- **THEN** the camera target is unchanged by flight
