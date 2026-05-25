export interface SsoValidateRequest {
  token: string;
}

export interface SsoValidateResponse {
  isValid: boolean;
  sessionId: number;
  userId: number;
  tenantId: number;
  applicationId: number;
  email: string;
  fullName: string;
  role: string;
}

export interface SessionStatus {
  active: boolean;
  sessionId: number;
  userId: number;
  tenantId: number;
  applicationId: number;
  email: string;
  fullName: string;
  role: string;
}

export interface StoredSession {
  sessionId: number;
  userId: number;
  tenantId: number;
  applicationId: number;
  email: string;
  fullName: string;
  role: string;
  signedInAt: string;
}
