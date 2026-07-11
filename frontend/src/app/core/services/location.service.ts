import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LocationOption } from '../models/purchase-bill.models';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);

  getLocations(): Observable<LocationOption[]> {
    return this.http.get<LocationOption[]>(`${environment.apiBaseUrl}/locations`);
  }
}
