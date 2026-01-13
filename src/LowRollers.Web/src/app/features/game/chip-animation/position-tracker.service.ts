import { Injectable } from '@angular/core';
import { type Position } from './chip-animation.models';

@Injectable({
  providedIn: 'root',
})
export class PositionTrackerService {
  /**
   * Resolve the on-screen center position of a player seat.
   * Seats are identified via a data-seat-position attribute in the DOM.
   */
  getSeatPosition(seatPosition: string): Position | null {
    const element = document.querySelector(`[data-seat-position="${seatPosition}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  /**
   * Resolve the position of the main pot.
   * Currently delegates to getMainPotPosition for clarity and future extensibility.
   */
  getPotPosition(): Position | null {
    return this.getMainPotPosition();
  }

  /**
   * Resolve the on-screen center position of the main pot element.
   * The main pot is identified via a data-pot-type="main" attribute.
   */
  getMainPotPosition(): Position | null {
    const element = document.querySelector('[data-pot-type="main"]');
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  /**
   * Resolve the on-screen center position of a side pot by index.
   * Used when side pots are rendered in a deterministic order.
   */
  getSidePotPosition(index: number): Position | null {
    const element = document.querySelector(`[data-pot-type="side"][data-pot-index="${index}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  /**
   * Resolve the on-screen center position of a side pot by unique pot id.
   * Useful when side pots are keyed by an identifier rather than index.
   */
  getSidePotPositionById(potId: string): Position | null {
    const element = document.querySelector(`[data-pot-type="side"][data-pot-id="${potId}"]`);
    if (!element) return null;
    return this.calculateCenterPosition(element as HTMLElement);
  }

  /**
   * Calculate the center point of an element in viewport coordinates.
   * Returns null if the element has no rendered size (e.g., not visible yet).
   */
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
