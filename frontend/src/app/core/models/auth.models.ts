export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserLocation {
  locationCode: string;
  locationName: string;
}

export interface AuthUser {
  email: string;
  locations: UserLocation[];
}
