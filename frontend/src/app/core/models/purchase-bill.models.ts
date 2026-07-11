export const allowedItems = [
  'Mango',
  'Apple',
  'Banana',
  'Orange',
  'Grapes',
  'Kiwi',
  'Strawberry'
] as const;

export type AllowedItem = (typeof allowedItems)[number];

export interface LocationOption {
  locationCode: string;
  locationName: string;
}

export interface PurchaseItemDraft {
  itemName: AllowedItem;
  locationCode: string;
  locationName: string;
  standardCost: number;
  standardPrice: number;
  quantity: number;
  discountPercentage: number;
}

export interface PurchaseBillLine extends PurchaseItemDraft {
  id: string;
  grossCost: number;
  discountAmount: number;
  totalCost: number;
  totalSelling: number;
}

export interface PurchaseBillSummary {
  totalItems: number;
  totalQuantity: number;
  totalCost: number;
  totalSelling: number;
}

export interface SavePurchaseBillRequest {
  items: PurchaseItemDraft[];
}

export interface SavePurchaseBillResponse {
  billId: string;
  totalItems: number;
  totalQuantity: number;
  totalCost: number;
  totalSelling: number;
}
