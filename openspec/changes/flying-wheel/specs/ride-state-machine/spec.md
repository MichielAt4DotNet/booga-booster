## ADDED Requirements

### Requirement: Flying lifecycle state

The ride lifecycle SHALL include a **Flying** state — the ride is airborne after a "yeet". Flying SHALL be reachable only from **Started** by an operator transition guarded by full mill and hub power (see `ride-flight`), and SHALL be listed in the available transitions only when that guard passes.

#### Scenario: Flying offered only when guard passes

- **WHEN** the ride is `Started` with mill and hub power at 100 %
- **THEN** `Flying` is among the available transitions

#### Scenario: Flying not offered otherwise

- **WHEN** the ride is `Started` and either power is below 100 %
- **THEN** `Flying` is not among the available transitions

## MODIFIED Requirements

### Requirement: Guarded, deterministic transitions

The ride SHALL move between states only along the transitions defined by the state machine, and SHALL reject any transition that is not defined for the current state. The legal **operator-triggered** transitions are:

| From | To | Guard |
| --- | --- | --- |
| Idle | Loading | none |
| Loading | Safe | ride is safe (every occupied restraint secured, load balanced, not overloaded) |
| Safe | Loading | none (reopen loading) |
| Safe | Started | ride is safe |
| Started | Flying | mill power is 100 % and hub power is 100 % |
| Started | Stopping | none |
| Flying | Stopping | none |
| Loading / Safe / Started / Flying / Stopping | Emergency Stop | none |

Any transition request whose (from, to) pair is not in this table, or whose guard is not satisfied, SHALL be rejected without changing state.

#### Scenario: Legal transition is accepted

- **WHEN** the ride is `Idle` and a transition to `Loading` is requested
- **THEN** the ride's state becomes `Loading`

#### Scenario: Undefined transition is rejected

- **WHEN** the ride is `Idle` and a transition to `Started` is requested
- **THEN** the request is rejected with a domain validation error
- **AND** the ride's state remains `Idle`

#### Scenario: Guarded transition blocked when unsafe

- **WHEN** the ride is `Loading` with at least one occupied seat whose restraint is not secured
- **AND** a transition to `Safe` is requested
- **THEN** the request is rejected with a domain validation error
- **AND** the ride's state remains `Loading`

#### Scenario: Guarded transition allowed when safe

- **WHEN** the ride is `Loading`, every occupied restraint is secured, the load is balanced and within the maximum, and a transition to `Safe` is requested
- **THEN** the ride's state becomes `Safe`
