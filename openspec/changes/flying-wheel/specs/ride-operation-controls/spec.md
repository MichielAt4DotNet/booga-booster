## ADDED Requirements

### Requirement: Yeet button

The *Lifecycle transitions* group SHALL contain a button labelled **"Yeet"** that requests the `Flying` transition. It SHALL be enabled only when `Flying` is among the transitions offered by the server — i.e. the ride is `Started`, not `Stopping`, and central mill power and hub power are both 100 % — and SHALL otherwise be disabled. The button SHALL decide nothing itself; a click only emits the transition request.

#### Scenario: Yeet enabled at full power

- **WHEN** the ride is `Started`, mill and hub power are 100 % and the server offers `Flying`
- **THEN** the "Yeet" button is enabled

#### Scenario: Yeet disabled below full power

- **WHEN** the ride is `Started` and the hub power is 90 %
- **THEN** the "Yeet" button is disabled

#### Scenario: Yeet disabled while stopping

- **WHEN** the ride is `Stopping`
- **THEN** the "Yeet" button is disabled

#### Scenario: Clicking Yeet requests the transition

- **WHEN** the operator activates the enabled "Yeet" button
- **THEN** a transition request to `flying` is emitted

#### Scenario: Yeet is accessible

- **WHEN** the dashboard is audited with AXE
- **THEN** the "Yeet" button has an accessible name, is keyboard operable and conveys its disabled state without relying on colour
