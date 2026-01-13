/** Supported animation semantics for chip flights. */
export type AnimationType = 'bet' | 'win' | 'pot-collect';

/** Simple 2D coordinate in viewport/screen space (px). */
export interface Position {
  x: number;
  y: number;
}

/**
 * Configuration describing a single chip-flight animation.
 * The service uses this to create chip instances and to drive CSS timing.
 */
export interface ChipAnimationConfig {
  /** Unique identifier for this animation instance. */
  id: string;

  /** Logical type of animation (used to select timing/visual treatment). */
  type: AnimationType;

  /** Start position for the chip flight (px). */
  fromPosition: Position;

  /** End position for the chip flight (px). */
  toPosition: Position;

  /** Amount represented by the animation (used to derive chip colors/count). */
  amount: number;

  /** Target number of chips to render (may be capped by defaults). */
  chipCount: number;

  /** Delay before the animation starts (ms). */
  delayMs: number;

  /** Duration of the chip flight (ms). */
  durationMs: number;
}

/**
 * Runtime representation of an in-flight animation.
 * Includes derived chip instances and timing metadata for cleanup/expiry.
 */
export interface ActiveChipAnimation {
  /** Immutable config used to render and time the animation. */
  config: ChipAnimationConfig;

  /** Timestamp captured when the animation was created (ms since epoch). */
  startTime: number;

  /** Per-chip render data (color, offsets, per-chip delay). */
  chips: AnimatedChip[];
}

/**
 * Per-chip instance data used by the overlay renderer.
 * Offsets introduce slight scatter so the chips do not overlap perfectly.
 */
export interface AnimatedChip {
  /** Unique identifier for the chip within an animation. */
  id: string;

  /** Chip color denomination used to select the SVG symbol. */
  color: 'white' | 'red' | 'blue' | 'green' | 'black';

  /** Random horizontal jitter applied to from/to positions (px). */
  offsetX: number;

  /** Random vertical jitter applied to from/to positions (px). */
  offsetY: number;

  /** Per-chip start delay used for staggered launches (ms). */
  delay: number;
}

/** Central defaults used for durations, staggering, and chip caps. */
export const ANIMATION_DEFAULTS = {
  betDurationMs: 400,
  winDurationMs: 600,
  chipStaggerMs: 30,
  maxChipsPerAnimation: 8,
};

export function generateAnimationId(): string {
  return `chip-anim-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Generate a list of chip instances based on an amount and a maximum chip count.
 * Uses a simple greedy breakdown by denomination and assigns jitter + stagger.
 */
export function calculateChipColors(amount: number, maxChips: number): AnimatedChip[] {
  const chips: AnimatedChip[] = [];
  const values: { color: AnimatedChip['color']; value: number }[] = [
    { color: 'black', value: 100 },
    { color: 'green', value: 25 },
    { color: 'blue', value: 10 },
    { color: 'red', value: 5 },
    { color: 'white', value: 1 },
  ];

  let remaining = amount;
  let chipIndex = 0;

  for (const { color, value } of values) {
    while (remaining >= value && chips.length < maxChips) {
      chips.push({
        id: `chip-${chipIndex}`,
        color,
        offsetX: (Math.random() - 0.5) * 20,
        offsetY: (Math.random() - 0.5) * 10,
        delay: chipIndex * ANIMATION_DEFAULTS.chipStaggerMs,
      });
      remaining -= value;
      chipIndex++;
    }
  }

  if (chips.length === 0 && amount > 0) {
    chips.push({
      id: 'chip-0',
      color: 'white',
      offsetX: 0,
      offsetY: 0,
      delay: 0,
    });
  }

  return chips;
}
