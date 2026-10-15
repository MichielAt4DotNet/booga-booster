# 6 — Flight ("Yeet")

A purely vertical, kinematic extension: once the ride is `Started` with the central mill
and every hub at **100 %** power, the operator may "yeet" it. The ride enters the
`Flying` state and its base lifts off the ground. There is no horizontal travel, landing
or crash; the rotational physics (docs 2–5) keeps running unchanged while airborne.

## 6.1 Climb model

Altitude `h` and climb speed `v` are advanced on the fixed step `dt = 1/120 s`
(semi-implicit Euler — speed first, then position from the new speed):

```
v ← min(v + a_lift · dt, v_max)
h ← h + v · dt
```

Starting from `h = 0`, `v = 0` this is constant acceleration `h = ½·a_lift·t²` until the
speed cap is reached at `t_cap = v_max / a_lift`, after which the climb is linear,
`h = h_cap + v_max·(t − t_cap)`. It is a pure function of state and `dt`, hence
deterministic.

## 6.2 Constants

| Constant | Value | Derivation |
|----------|------:|------------|
| `FlightLiftAcceleration` | 2 m/s² | A brisk but visible lift-off (~0.2 g): 2 m is cleared in ~1.4 s, so the take-off reads clearly on screen without feeling instantaneous. |
| `FlightClimbSpeedMax` | 15 m/s | Cap on the ramp; reached after `15 / 2 = 7.5 s` at `h = ½·2·7.5² ≈ 56 m`, keeping the climb bounded and the 3D view framed. |
| `FlightCameraFollowAltitude` | 3 m | Reached after `√(2·3/2) ≈ 1.7 s` — high enough that the base is clearly off the ground before the camera starts tracking it. Mirrored by the frontend. |

## 6.3 Leaving flight

`Flying → Stopping` and `Flying → Emergency Stop` are allowed; the ride never returns to
`Started`. Lift ceases on leaving `Flying` and the altitude is **held** (no landing). The
ride settles through the normal stop logic (doc 3) and the altitude resets to 0 m when
the ride returns to `Idle`.
