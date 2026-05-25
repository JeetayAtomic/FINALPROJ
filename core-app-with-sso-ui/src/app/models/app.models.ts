export interface ApplicationDto {
  id: number;
  name: string;
  description: string;
  baseUrl: string;
  iconName: string;
  iconColor: string;
  displayOrder: number;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface TenantSummary {
  tenantId: number;
  slug: string;
  name: string;
  role: string;
  logoUrl?: string | null;
}

/** Response from POST /auth/login. */
export interface LoginResponse {
  isSuperAdmin: boolean;
  /** Populated only for super admins (tenant-less JWT). */
  token: string;
  /** Populated for regular users awaiting tenant selection. */
  interimToken: string;
  fullName: string;
  email: string;
  tenants: TenantSummary[];
}

export interface SelectTenantDto {
  tenantId: number;
}

/** Tenant-scoped JWT response (after select-tenant). */
export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  tenantId: number;
  tenantName: string;
  role: string;
  tenantLogoUrl?: string | null;
  /** Server-side session id for cross-app logout sync. */
  sessionId: number;
}

export interface SessionStatus {
  active: boolean;
  sessionId: number;
}

export interface SSORedirect {
  redirectUrl: string;
  ssoToken: string;
}

// ---------- Super-admin tenant management ----------

export interface TenantDto {
  id: number;
  name: string;
  slug: string;
  dbName: string;
  clientCode: string;
  templateId?: string | null;
  organizationId?: string | null;

  clientFolderPath?: string | null;
  rawFolderPath?: string | null;
  processedFolderPath?: string | null;
  errorFolderPath?: string | null;
  publishFolderPath?: string | null;
  jsonFolderPath?: string | null;
  logoUrl?: string | null;

  isActive: boolean;
  createdDate: string;
  createdBy: string;
  lastUpdatedDate?: string | null;
  lastUpdatedBy?: string | null;
}

export interface TenantWriteBase {
  name: string;
  dbName: string;
  clientCode: string;
  templateId?: string | null;
  organizationId?: string | null;
  clientFolderPath?: string | null;
  rawFolderPath?: string | null;
  processedFolderPath?: string | null;
  errorFolderPath?: string | null;
  publishFolderPath?: string | null;
  jsonFolderPath?: string | null;
  logoUrl?: string | null;
}

export interface CreateTenantDto extends TenantWriteBase {
  initialAdminFullName: string;
  initialAdminEmail: string;
  initialAdminPassword: string;
}

export interface UpdateTenantDto extends TenantWriteBase {
  isActive: boolean;
}

export interface SeedTenantUserDto {
  fullName: string;
  email: string;
  password?: string | null;
  roleName: string;
}

// ---------- Tenant-admin: users ----------

export interface TenantUserDto {
  userId: number;
  fullName: string;
  email: string;
  roleName: string;
  isActive: boolean;
  membershipCreatedAt: string;
}

export interface CreateTenantUserDto {
  fullName: string;
  email: string;
  password?: string | null;
  roleName: string;
}

export interface UpdateTenantUserDto {
  fullName: string;
  roleName: string;
  isActive: boolean;
}

export interface ResetPasswordDto {
  password: string;
}

// ---------- Tenant-admin: roles ----------

export interface RoleDto {
  id: number;
  name: string;
  description?: string | null;
  isSystem: boolean;
  createdAt: string;
  applicationIds: number[];
  memberCount?: number;
}

export interface CreateRoleDto {
  name: string;
  description?: string | null;
  applicationIds: number[];
}

export interface UpdateRoleDto extends CreateRoleDto {}

// ---------- Tenant-admin: JSON templates ----------

export interface JsonTemplateSummary {
  id: number;
  name: string;
  description?: string | null;
  version: number;
  createdAt: string;
  createdBy: string;
  lastUpdatedAt?: string | null;
  lastUpdatedBy?: string | null;
}

export interface JsonTemplateDto extends JsonTemplateSummary {
  jsonContent: string;
}

export interface CreateJsonTemplateDto {
  name: string;
  description?: string | null;
  jsonContent: string;
}

export interface UpdateJsonTemplateDto {
  name: string;
  description?: string | null;
  jsonContent: string;
}

// ---------- Tenant-admin: API keys (machine-to-machine) ----------

export interface ApiKeySummary {
  id: number;
  name: string;
  keyPrefix: string;
  scopes: string[];
  createdDate: string;
  createdBy: string;
  expiresAt?: string | null;
  lastUsedAt?: string | null;
  revokedAt?: string | null;
  isActive: boolean;
}

export interface CreateApiKeyDto {
  name: string;
  scopes?: string[] | null;
  expiresAt?: string | null;
}

export interface CreateApiKeyResponse extends ApiKeySummary {
  /** Returned exactly once on create; capture on the client and show to the user. */
  fullKey: string;
}

// ---------- Super-admin: cross-tenant templates + lookup ----------

export interface AdminTemplateRow {
  tenantId: number;
  tenantName: string;
  clientCode: string;
  templateId: number;
  templateName: string;
  description?: string | null;
  version: number;
  createdAt: string;
  createdBy: string;
  lastUpdatedAt?: string | null;
  lastUpdatedBy?: string | null;
}

export interface AdminLookupResponse {
  tenant: TenantDto;
  template: JsonTemplateDto;
}

// ---------- Super-admin: application catalog ----------

export interface ApplicationAdminDto {
  id: number;
  name: string;
  description: string;
  baseUrl: string;
  iconName: string;
  iconColor: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
}

export interface ApplicationWriteBase {
  name: string;
  description: string;
  baseUrl: string;
  iconName: string;
  iconColor: string;
  displayOrder: number;
}

export interface CreateApplicationDto extends ApplicationWriteBase {}

export interface UpdateApplicationDto extends ApplicationWriteBase {
  isActive: boolean;
}

// ---------- Tenant subscriptions (super-admin + tenant-admin) ----------

export interface TenantApplicationDto {
  applicationId: number;
  name: string;
  description: string;
  iconName: string;
  iconColor: string;
  displayOrder: number;
  applicationActive: boolean;
  isSubscribed: boolean;
  subscriptionActive: boolean;
  assignedAt?: string | null;
}

export interface TenantApplicationWriteDto {
  isActive: boolean;
}
