import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChipAnimationService } from './chip-animation.service';
import { getChipSymbolId } from '../../../shared/models/chip.models';

@Component({
  selector: 'app-chip-animation',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="chip-animation-layer">
      @for (animation of animations(); track animation.config.id) {
        @for (chip of animation.chips; track chip.id) {import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChipAnimationService } from './chip-animation.service';
import { getChipSymbolId } from '../../../shared/models/chip.models';

/**
 * Overlay component responsible for rendering chip-flight animations across the UI.
 * Animations are driven by ChipAnimationService, which maintains the current list
 * of active animations and their chip instances.
 */
@Component({
  selector: 'app-chip-animation',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Full-screen, non-interactive overlay layer for chip animations -->
    <div class="chip-animation-layer">
      @for (animation of animations(); track animation.config.id) {
        @for (chip of animation.chips; track chip.id) {
          <div
            class="flying-chip"
            [class.win-animation]="animation.config.type === 'win'"
            [style.--from-x.px]="animation.config.fromPosition.x + chip.offsetX"
            [style.--from-y.px]="animation.config.fromPosition.y + chip.offsetY"
            [style.--to-x.px]="animation.config.toPosition.x + chip.offsetX"
            [style.--to-y.px]="animation.config.toPosition.y + chip.offsetY"
            [style.--duration.ms]="animation.config.durationMs"
            [style.--delay.ms]="chip.delay"
            [style.animation-delay.ms]="chip.delay"
          >
            <!-- Render chip via SVG symbol reference to avoid duplicating SVG markup per chip -->
            <svg viewBox="0 0 100 100">
              <use [attr.href]="getChipSymbolId(chip.color)" />
            </svg>
          </div>
        }
      }
    </div>
  `,
  styles: [
    `
      /* Fixed overlay that spans the viewport and does not intercept mouse/touch events */
      .chip-animation-layer {
        position: fixed;
        inset: 0;
        pointer-events: none;
        z-index: 1000;
        overflow: visible;
      }

      /* Individual chip element that animates from a start point to an end point */
      .flying-chip {
        position: absolute;
        width: 28px;
        height: 28px;
        left: 0;
        top: 0;
        animation: flyChip var(--duration, 400ms) ease-out forwards;
        animation-delay: var(--delay, 0ms);
        opacity: 0;
        transform: translate(var(--from-x, 0), var(--from-y, 0));
      }

      /* Shadow to help chips read against the table felt and background */
      .flying-chip svg {
        width: 100%;
        height: 100%;
        filter: drop-shadow(0 4px 6px rgba(0, 0, 0, 0.5));
      }

      /* Alternative keyframe set for "win" animations (more dramatic arc + rotation) */
      .flying-chip.win-animation {
        animation: flyChipWin var(--duration, 600ms) ease-in-out forwards;
      }

      /* Standard chip travel: arced midpoint path using CSS calc() between from/to */
      @keyframes flyChip {
        0% {
          opacity: 1;
          transform: translate(var(--from-x), var(--from-y)) scale(1);
        }
        50% {
          transform: translate(
              calc((var(--from-x) + var(--to-x)) / 2),
              calc((var(--from-y) + var(--to-y)) / 2 - 30px)
            )
            scale(1.1);
        }
        100% {
          opacity: 1;
          transform: translate(var(--to-x), var(--to-y)) scale(1);
        }
      }

      /* Win travel: staged path segments with rotation for a more celebratory feel */
      @keyframes flyChipWin {
        0% {
          opacity: 1;
          transform: translate(var(--from-x), var(--from-y)) scale(1) rotate(0deg);
        }
        25% {
          transform: translate(
              calc(var(--from-x) + (var(--to-x) - var(--from-x)) * 0.25),
              calc(var(--from-y) + (var(--to-y) - var(--from-y)) * 0.25 - 50px)
            )
            scale(1.2)
            rotate(180deg);
        }
        75% {
          transform: translate(
              calc(var(--from-x) + (var(--to-x) - var(--from-x)) * 0.75),
              calc(var(--from-y) + (var(--to-y) - var(--from-y)) * 0.75 - 20px)
            )
            scale(1.1)
            rotate(360deg);
        }
        100% {
          opacity: 1;
          transform: translate(var(--to-x), var(--to-y)) scale(1) rotate(360deg);
        }
      }
    `,
  ],
})
export class ChipAnimationComponent {
  private chipAnimationService = inject(ChipAnimationService);

  /** Reactive list of active animations currently being rendered. */
  animations = computed(() => this.chipAnimationService.activeAnimations());

  /** Map a chip color to the SVG symbol href used by the <use> element. */
  getChipSymbolId = getChipSymbolId;
}

          <div
            class="flying-chip"
            [class.win-animation]="animation.config.type === 'win'"
            [style.--from-x.px]="animation.config.fromPosition.x + chip.offsetX"
            [style.--from-y.px]="animation.config.fromPosition.y + chip.offsetY"
            [style.--to-x.px]="animation.config.toPosition.x + chip.offsetX"
            [style.--to-y.px]="animation.config.toPosition.y + chip.offsetY"
            [style.--duration.ms]="animation.config.durationMs"
            [style.--delay.ms]="chip.delay"
            [style.animation-delay.ms]="chip.delay"
          >
            <svg viewBox="0 0 100 100">
              <use [attr.href]="getChipSymbolId(chip.color)" />
            </svg>
          </div>
        }
      }
    </div>
  `,
  styles: [
    `
      .chip-animation-layer {
        position: fixed;
        inset: 0;
        pointer-events: none;
        z-index: 1000;
        overflow: visible;
      }

      .flying-chip {
        position: absolute;
        width: 28px;
        height: 28px;
        left: 0;
        top: 0;
        animation: flyChip var(--duration, 400ms) ease-out forwards;
        animation-delay: var(--delay, 0ms);
        opacity: 0;
        transform: translate(var(--from-x, 0), var(--from-y, 0));
      }

      .flying-chip svg {
        width: 100%;
        height: 100%;
        filter: drop-shadow(0 4px 6px rgba(0, 0, 0, 0.5));
      }

      .flying-chip.win-animation {
        animation: flyChipWin var(--duration, 600ms) ease-in-out forwards;
      }

      @keyframes flyChip {
        0% {
          opacity: 1;
          transform: translate(var(--from-x), var(--from-y)) scale(1);
        }
        50% {
          transform: translate(
              calc((var(--from-x) + var(--to-x)) / 2),
              calc((var(--from-y) + var(--to-y)) / 2 - 30px)
            )
            scale(1.1);
        }
        100% {
          opacity: 1;
          transform: translate(var(--to-x), var(--to-y)) scale(1);
        }
      }

      @keyframes flyChipWin {
        0% {
          opacity: 1;
          transform: translate(var(--from-x), var(--from-y)) scale(1) rotate(0deg);
        }
        25% {
          transform: translate(
              calc(var(--from-x) + (var(--to-x) - var(--from-x)) * 0.25),
              calc(var(--from-y) + (var(--to-y) - var(--from-y)) * 0.25 - 50px)
            )
            scale(1.2)
            rotate(180deg);
        }
        75% {
          transform: translate(
              calc(var(--from-x) + (var(--to-x) - var(--from-x)) * 0.75),
              calc(var(--from-y) + (var(--to-y) - var(--from-y)) * 0.75 - 20px)
            )
            scale(1.1)
            rotate(360deg);
        }
        100% {
          opacity: 1;
          transform: translate(var(--to-x), var(--to-y)) scale(1) rotate(360deg);
        }
      }
    `,
  ],
})
export class ChipAnimationComponent {
  private chipAnimationService = inject(ChipAnimationService);

  animations = computed(() => this.chipAnimationService.activeAnimations());
  getChipSymbolId = getChipSymbolId;

}
