/**
 * Public barrel exports for the chip animation subsystem.
 * Consumers should import from this index rather than individual files
 * to keep dependencies stable and encapsulated.
 */

/** Overlay component responsible for rendering chip-flight animations. */
export { ChipAnimationComponent } from './chip-animation.component';

/** Service used to trigger and manage chip animations across the app. */
export { ChipAnimationService } from './chip-animation.service';

/** Utility service for tracking DOM element positions in screen space. */
export { PositionTrackerService } from './position-tracker.service';

/**
 * Core animation models, types, and helpers.
 * These define the animation contracts and chip generation logic.
 */
export {
  type AnimationType,
  type Position,
  type ChipAnimationConfig,
  type ActiveChipAnimation,
  type AnimatedChip,
  ANIMATION_DEFAULTS,
  generateAnimationId,
  calculateChipColors,
} from './chip-animation.models';
