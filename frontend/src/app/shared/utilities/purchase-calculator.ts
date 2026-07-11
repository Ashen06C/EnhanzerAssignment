import {
  PurchaseBillLine,
  PurchaseBillSummary,
  PurchaseItemDraft
} from '../../core/models/purchase-bill.models';

const roundMoney = (value: number): number => Math.round((value + Number.EPSILON) * 100) / 100;

export const calculateLine = (draft: PurchaseItemDraft, id: string = crypto.randomUUID()): PurchaseBillLine => {
  const grossCost = roundMoney(draft.standardCost * draft.quantity);
  const discountAmount = roundMoney((grossCost * draft.discountPercentage) / 100);
  const totalCost = roundMoney(grossCost - discountAmount);
  const totalSelling = roundMoney(draft.standardPrice * draft.quantity);

  return {
    ...draft,
    id,
    grossCost,
    discountAmount,
    totalCost,
    totalSelling
  };
};

export const summarizeLines = (lines: PurchaseBillLine[]): PurchaseBillSummary => ({
  totalItems: lines.length,
  totalQuantity: roundMoney(lines.reduce((sum, line) => sum + line.quantity, 0)),
  totalCost: roundMoney(lines.reduce((sum, line) => sum + line.totalCost, 0)),
  totalSelling: roundMoney(lines.reduce((sum, line) => sum + line.totalSelling, 0))
});
