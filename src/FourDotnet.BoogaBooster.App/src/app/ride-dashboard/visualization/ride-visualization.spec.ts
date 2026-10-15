import { TestBed } from '@angular/core/testing';

import {
  RideVisualization,
  angularVelocity,
  cameraFollowDelta,
  liftAltitude,
  podFreeAngle,
  rpmToRadPerSec,
} from './ride-visualization';

describe('flight helpers', () => {
  it('lifts by the altitude and treats bad values as grounded', () => {
    expect(liftAltitude(7.5)).toBe(7.5);
    expect(liftAltitude(-2)).toBe(0);
    expect(liftAltitude(NaN)).toBe(0);
    expect(liftAltitude(Infinity)).toBe(0);
  });

  it('does not move the camera below the follow threshold', () => {
    expect(cameraFollowDelta(0, 5.5)).toBe(0);
    expect(cameraFollowDelta(2.99, 5.5)).toBe(0);
  });

  it('moves the target onto the ride once altitude reaches the threshold', () => {
    expect(cameraFollowDelta(3, 5.5)).toBeCloseTo(3);
    expect(cameraFollowDelta(10, 8.5)).toBeCloseTo(7);
  });

  it('is idempotent once the target has caught up, so user orbit is left alone', () => {
    expect(cameraFollowDelta(10, 15.5)).toBeCloseTo(0);
  });
});

describe('RideVisualization', () => {
  function render() {
    const fixture = TestBed.createComponent(RideVisualization);
    fixture.componentRef.setInput('millSpeedRpm', 6);
    fixture.componentRef.setInput('millDirection', 'forward');
    fixture.componentRef.setInput('hubSpeedRpm', 9);
    fixture.componentRef.setInput('hubDirection', 'reverse');
    fixture.componentRef.setInput('gondolaBrakeEngaged', false);
    fixture.componentRef.setInput('flightAltitudeMeters', 5);
    fixture.detectChanges();
    return fixture;
  }

  it('mounts without a WebGL context and stays hidden from assistive tech', () => {
    const fixture = render();
    const host = fixture.nativeElement as HTMLElement;

    expect(host.getAttribute('aria-hidden')).toBe('true');
    expect(host.querySelector('.viz')).not.toBeNull();
  });

  it('maps rpm to radians per second', () => {
    expect(rpmToRadPerSec(60)).toBeCloseTo(2 * Math.PI, 5);
    expect(rpmToRadPerSec(0)).toBe(0);
  });

  it('derives angular velocity from signed rpm', () => {
    expect(angularVelocity(60)).toBeCloseTo(2 * Math.PI, 5);
    expect(angularVelocity(-60)).toBeCloseTo(-2 * Math.PI, 5);
    expect(angularVelocity(0)).toBe(0);
  });

  it('coerces a non-finite rpm to 0 instead of corrupting the rotation', () => {
    expect(angularVelocity(NaN)).toBe(0);
    expect(angularVelocity(Infinity)).toBe(0);
    expect(angularVelocity(-Infinity)).toBe(0);
  });

  it('advances the free pod trajectory with the combined rotation', () => {
    // At rest the pod sits at its phase-shifted swing baseline.
    expect(podFreeAngle(0, 0)).toBe(0);
    expect(podFreeAngle(0, Math.PI / 2)).toBeCloseTo(1, 5);
    // The trajectory counter-rotates against the combined spin.
    expect(podFreeAngle(Math.PI, 0)).toBeCloseTo(-Math.PI, 5);
  });
});
