import { Injectable } from '@angular/core';
import { type Position } from './chip-animation.models';

@Injectable({
  providedIn: 'root',
})
export class PositionTrackerService {
  getSeatPosition(seatPosition: string): Position | null {
    const element = document.querySelector(`[data-seat-position="${seatPosition}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  getPotPosition(): Position | null {
    return this.getMainPotPosition();
  }

  getMainPotPosition(): Position | null {
    const element = document.querySelector('[data-pot-type="main"]');
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  getSidePotPosition(index: number): Position | null {
    const element = document.querySelector(`[data-pot-type="side"][data-pot-index="${index}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  getSidePotPositionById(potId: string): Position | null {
    const element = document.querySelector(`[data-pot-type="side"][data-pot-id="${potId}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  private calculateCenterPosition(element: HTMLElement): Position | null {
    const rect = element.getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) {
      return null;
    }
    return {
      x: rect.left + rect.width / 2,
      y: rect.top + rect.height / 2,
    };
  }
}