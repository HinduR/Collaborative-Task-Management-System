export interface AccessTokenResponse {
  accessToken: string;
  expiresIn: number;
}

export interface GoogleLoginCodeExchangeRequest {
  code: string;
}