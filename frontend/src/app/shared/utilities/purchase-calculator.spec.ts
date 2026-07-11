import { describe, expect, it } from 'vitest';
import { calculateLine, summarizeLines } from './purchase-calculator';

describe('purchase calculator', () => {
  it('calculates the PDF example totals', () => {
    const line = calculateLine(
      {
        itemName: 'Mango',
        locationCode: 'LOC01',
        locationName: 'Main Warehouse',
        standardCost: 100,
        standardPrice: 150,
        quantity: 5,
        discountPercentage: 20
      },
      'line-1'
    );

    expect(line.totalCost).toBe(400);
    expect(line.totalSelling).toBe(750);
  });

  it('summarizes rows without duplicated state', () => {
    const first = calculateLine(
      {
        itemName: 'Apple',
        locationCode: 'A',
        locationName: 'A',
        standardCost: 10,
        standardPrice: 12,
        quantity: 2,
        discountPercentage: 0
      },
      'a'
    );
    const second = calculateLine(
      {
        itemName: 'Kiwi',
        locationCode: 'B',
        locationName: 'B',
        standardCost: 20,
        standardPrice: 30,
        quantity: 3,
        discountPercentage: 50
      },
      'b'
    );

    expect(summarizeLines([first, second])).toEqual({
      totalItems: 2,
      totalQuantity: 5,
      totalCost: 50,
      totalSelling: 114
    });
  });
});
