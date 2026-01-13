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
  private _activeAnimations = signal<ActiveChipAnimation[]>([]);
  readonly activeAnimations = this._activeAnimations.asReadonly();

  animateBet(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('bet', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.betDurationMs);
  }

  animateWin(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('win', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.winDurationMs);
  }

  animatePotCollect(fromPosition: Position, toPosition: Position, amount: number): string {
    return this.createAnimation('pot-collect', fromPosition, toPosition, amount, ANIMATION_DEFAULTS.betDurationMs);
  }

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

  private removeAnimation(id: string): void {
    this._activeAnimations.update((anims) => anims.filter((a) => a.config.id !== id));
  }

  clearAll(): void {
    this._activeAnimations.set([]);
  }
}