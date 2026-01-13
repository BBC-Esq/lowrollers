import { Injectable, signal } from '@angular/core';
import {
  type ChipAnimationConfig,
  type ActiveChipAnimation,
  type Position,
  type AnimationType,
  ANIMATION_DEFAULTS,
  generateAnimationId,
  calculateChipColors,
} from './chip-animation.models';

@Injectable({
  providedIn: 'root',
})
export class ChipAnimationService {
  /** Internal signal holding the currently active chip-flight animations. */
  private _activeAnimations = signal<ActiveChipAnimation[]>([]);

  /** Read-only view of active animations for renderers (e.g., ChipAnimationComponent). */
  readonly activeAnimations = this._activeAnimations.asReadonly();

  /** Animate chips moving from a player/seat position to the pot (or similar bet target). */
  animateBet(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('bet', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.betDurationMs);
  }

  /** Animate chips moving to a winner/seat position (uses the win duration styling/timing). */
  animateWin(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('win', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.winDurationMs);
  }

  /** Animate chips moving from the pot to a destination (e.g., winner collects pot). */
  animatePotCollect(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('pot-collect', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.betDurationMs);
  }

  /**
   * Create and register a new animation instance.
   * Generates chip instances (colors, offsets, stagger delays) and schedules removal once complete.
   */
  private createAnimation(
    type: AnimationType,
    fromPosition: Position,
    toPosition: Position,
    amount: number,
    durationMs: number
  ): string {
    const id = generateAnimationId();
    const chips = calculateChipColors(amount, ANIMATION_DEFAULTS.maxChipsPerAnimation);

    const config: ChipAnimationConfig = {
      id,
      type,
      fromPosition,
      toPosition,
      amount,
      chipCount: chips.length,
      delayMs: 0,
      durationMs,
    };

    const animation: ActiveChipAnimation = {
      config,
      startTime: Date.now(),
      chips,
    };

    this._activeAnimations.update((anims) => [...anims, animation]);

    const totalDuration = durationMs + chips.length * ANIMATION_DEFAULTS.chipStaggerMs + 100;
    setTimeout(() => this.removeAnimation(id), totalDuration);

    return id;
  }

  /** Remove a specific animation by id once it has completed. */
  private removeAnimation(id: string): void {
    this._activeAnimations.update((anims) => anims.filter((a) => a.config.id !== id));
  }

  /** Immediately clear all active animations (e.g., route change, table reset). */
  clearAll(): void {
    this._activeAnimations.set([]);
  }
}
