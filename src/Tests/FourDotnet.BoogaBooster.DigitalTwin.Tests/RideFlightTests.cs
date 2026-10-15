using FourDotnet.BoogaBooster.Core;
using FourDotnet.BoogaBooster.DigitalTwin.Abstractions;
using FourDotnet.BoogaBooster.DigitalTwin.Domain;
using Xunit;

namespace FourDotnet.BoogaBooster.DigitalTwin.Tests;

/// <summary>The "Yeet" transition guard and the deterministic take-off and climb.</summary>
public sealed class RideFlightTests
{
    [Fact]
    public void Flying_is_appended_at_index_seven()
    {
        Assert.Equal(7, (int)RideState.Flying);
        Assert.Equal("Flying", RideState.Flying.ToString());
    }

    [Fact]
    public void Yeet_is_accepted_at_full_mill_and_hub_power()
    {
        var ride = StartedRide(millPercent: 100, hubPercent: 100);

        Assert.Contains(RideState.Flying, ride.AvailableTransitions);
        ride.RequestTransition(RideState.Flying);

        Assert.Equal(RideState.Flying, ride.CurrentState);
    }

    [Theory]
    [InlineData(99, 100)]
    [InlineData(100, 80)]
    [InlineData(0, 0)]
    public void Yeet_is_rejected_and_not_offered_below_full_power(double mill, double hub)
    {
        var ride = StartedRide(mill, hub);

        Assert.DoesNotContain(RideState.Flying, ride.AvailableTransitions);
        Assert.Throws<DomainValidationException>(() => ride.RequestTransition(RideState.Flying));
        Assert.Equal(RideState.Started, ride.CurrentState);
    }

    [Theory]
    [InlineData(RideState.Idle)]
    [InlineData(RideState.Loading)]
    [InlineData(RideState.Safe)]
    [InlineData(RideState.Stopping)]
    public void Yeet_is_rejected_from_any_state_but_started(RideState from)
    {
        var ride = Ride.Create();
        ride.SetMainEnginePower(new EnginePower(100));
        ride.SetHubEnginePower(new EnginePower(100));
        DriveTo(ride, from);

        Assert.DoesNotContain(RideState.Flying, ride.AvailableTransitions);
        Assert.Throws<DomainValidationException>(() => ride.RequestTransition(RideState.Flying));
        Assert.Equal(from, ride.CurrentState);
    }

    [Fact]
    public void A_flying_ride_offers_only_stopping_and_emergency_stop()
    {
        var ride = FlyingRide();

        Assert.Equal(new[] { RideState.Stopping, RideState.EmergencyStop }, ride.AvailableTransitions);
        Assert.Throws<DomainValidationException>(() => ride.RequestTransition(RideState.Started));
        Assert.True(ride.IsRunning);
        Assert.True(ride.ConstraintsLocked);
    }

    [Fact]
    public void Altitude_is_zero_when_never_flown()
    {
        var ride = StartedRide(100, 100);
        RideSafetyTests.Advance(ride, seconds: 2d);

        Assert.Equal(0d, ride.FlightAltitudeMeters);
        Assert.Equal(0d, ride.ToTelemetry().FlightAltitudeMeters);
    }

    [Fact]
    public void Altitude_rises_monotonically_while_flying()
    {
        var ride = FlyingRide();
        var previous = ride.FlightAltitudeMeters;

        for (var i = 0; i < 5 * 120; i++)
        {
            ride.Advance(TestHelpers.Dt);
            Assert.True(ride.FlightAltitudeMeters >= previous);
            previous = ride.FlightAltitudeMeters;
        }

        Assert.True(ride.FlightAltitudeMeters > 0d);
        Assert.Equal(ride.FlightAltitudeMeters, ride.ToTelemetry().FlightAltitudeMeters);
    }

    [Fact]
    public void Early_climb_matches_constant_acceleration()
    {
        var ride = FlyingRide();

        RideSafetyTests.Advance(ride, seconds: 2d);

        // h = ½·a·t² (semi-implicit Euler adds ~a·t·dt/2 ≈ 1.7 cm over 2 s).
        var expected = 0.5d * RideParameters.FlightLiftAcceleration * 4d;
        Assert.Equal(expected, ride.FlightAltitudeMeters, tolerance: 0.05d);
    }

    [Fact]
    public void Climb_speed_is_capped_at_the_maximum()
    {
        var ride = FlyingRide();
        RideSafetyTests.Advance(ride, seconds: 30d);
        var before = ride.FlightAltitudeMeters;

        RideSafetyTests.Advance(ride, seconds: 1d);

        // Well past the ramp-up (15 / 2 = 7.5 s) the ride gains exactly v_max per second.
        Assert.Equal(RideParameters.FlightClimbSpeedMax, ride.FlightAltitudeMeters - before, tolerance: 0.05d);
    }

    [Fact]
    public void Two_rides_given_the_same_steps_fly_identically()
    {
        var first = FlyingRide();
        var second = FlyingRide();

        for (var i = 0; i < 10 * 120; i++)
        {
            first.Advance(TestHelpers.Dt);
            second.Advance(TestHelpers.Dt);
        }

        Assert.Equal(first.FlightAltitudeMeters, second.FlightAltitudeMeters);
    }

    [Theory]
    [InlineData(RideState.EmergencyStop)]
    [InlineData(RideState.Stopping)]
    public void Leaving_flying_stops_the_climb_and_holds_the_altitude(RideState target)
    {
        var ride = FlyingRide();
        RideSafetyTests.Advance(ride, seconds: 2d);

        ride.RequestTransition(target);
        var held = ride.FlightAltitudeMeters;
        RideSafetyTests.Advance(ride, seconds: 0.1d);

        // The mill is still braking, so the ride has not settled back to Idle yet.
        Assert.NotEqual(RideState.Idle, ride.CurrentState);
        Assert.True(held > 0d);
        Assert.Equal(held, ride.FlightAltitudeMeters);
        Assert.Throws<DomainValidationException>(() => ride.RequestTransition(RideState.Started));
    }

    [Fact]
    public void A_stopped_flying_ride_settles_and_the_altitude_resets_once_idle()
    {
        var ride = FlyingRide();
        RideSafetyTests.Advance(ride, seconds: 2d);
        ride.RequestTransition(RideState.Stopping);

        RideSafetyTests.Advance(ride, seconds: 120d);

        Assert.Equal(RideState.Idle, ride.CurrentState);
        Assert.Equal(0d, ride.FlightAltitudeMeters);
    }

    private static Ride StartedRide(double millPercent, double hubPercent)
    {
        var ride = Ride.Create();
        ride.SetMainEnginePower(new EnginePower(millPercent));
        ride.SetHubEnginePower(new EnginePower(hubPercent));
        DriveTo(ride, RideState.Started);
        return ride;
    }

    private static Ride FlyingRide()
    {
        var ride = StartedRide(100, 100);
        ride.RequestTransition(RideState.Flying);
        return ride;
    }

    private static void DriveTo(Ride ride, RideState state)
    {
        if (state == RideState.Idle)
        {
            return;
        }

        ride.RequestTransition(RideState.Loading);
        if (state == RideState.Loading)
        {
            return;
        }

        ride.RequestTransition(RideState.Safe);
        if (state == RideState.Safe)
        {
            return;
        }

        ride.RequestTransition(RideState.Started);
        if (state == RideState.Stopping)
        {
            ride.RequestTransition(RideState.Stopping);
        }
    }
}
