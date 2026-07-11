import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SavePurchaseBillRequest,
  SavePurchaseBillResponse
} from '../models/purchase-bill.models';

@Injectable({ providedIn: 'root' })
export class PurchaseBillApiService {
  private readonly http = inject(HttpClient);

  saveBill(request: SavePurchaseBillRequest): Observable<SavePurchaseBillResponse> {
    return this.http.post<SavePurchaseBillResponse>(`${environment.apiBaseUrl}/purchase-bills`, request);
  }
}
