## ADDED Requirements

### Requirement: Airborne 3D view

The ride visualization SHALL draw the base at the telemetry flight altitude above the ground, and once the altitude reaches the follow threshold the camera SHALL track the ride while still allowing the user to orbit and zoom around it. The ground plane SHALL stay in place.

#### Scenario: Ground stays behind

- **WHEN** the ride is flying
- **THEN** the ground plane remains at height 0 while the base and rig rise

#### Scenario: Orbit still works while following

- **WHEN** the camera is following the ride and the user drags to orbit
- **THEN** the camera orbits around the ride rather than snapping back
