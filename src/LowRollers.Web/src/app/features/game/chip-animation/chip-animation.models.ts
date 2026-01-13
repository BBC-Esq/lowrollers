export type AnimationType = 'bet' | 'win' | 'pot-collect';

export interface Position {
  x: number;
  y: number;
}

export interface ChipAnimationConfig {
  id: string;
  type: AnimationType;
  fromPosition: Position;
  toPosition: Position;
  amount: number;
  chipCount: number;
  delayMs: number;
  durationMs: number;
}

export interface ActiveChipAnimation {
  config: ChipAnimationConfig;
  startTime: number;
  chips: AnimatedChip[];
}

export interface AnimatedChip {
  id: string;
  color: 'white' | 'red' | 'blue' | 'green' | 'black';
  offsetX: number;
  offsetY: number;
  delay: number;
}

export const ANIMATION_DEFAULTS = {
  betDurationMs: 400,
  winDurationMs: 600,
  chipStaggerMs: 30,
  maxChipsPerAnimation: 8,
};

export function generateAnimationId(): string {
  return `chip-anim-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

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