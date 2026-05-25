import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TenantSelectComponent } from './components/tenant-select/tenant-select.component';
import { TenantsListComponent } from './components/admin/tenants-list/tenants-list.component';
import { TenantFormComponent } from './components/admin/tenant-form/tenant-form.component';
import { AdminTemplatesComponent } from './components/admin/admin-templates/admin-templates.component';
import { AdminLookupComponent } from './components/admin/admin-lookup/admin-lookup.component';
import { ApplicationsListComponent } from './components/admin/applications-list/applications-list.component';
import { ApplicationFormComponent } from './components/admin/application-form/application-form.component';
import { TenantApplicationsComponent } from './components/admin/tenant-applications/tenant-applications.component';
import { TenantAdminApplicationsListComponent } from './components/tenant-admin/applications-list/applications-list.component';
import { UsersListComponent } from './components/tenant-admin/users-list/users-list.component';
import { UserFormComponent } from './components/tenant-admin/user-form/user-form.component';
import { RolesListComponent } from './components/tenant-admin/roles-list/roles-list.component';
import { RoleFormComponent } from './components/tenant-admin/role-form/role-form.component';
import { TemplatesListComponent } from './components/tenant-admin/templates-list/templates-list.component';
import { TemplateEditorComponent } from './components/tenant-admin/template-editor/template-editor.component';
import { ApiKeysListComponent } from './components/tenant-admin/api-keys-list/api-keys-list.component';
import { authGuard } from './guards/auth.guard';
import { superAdminGuard } from './guards/super-admin.guard';
import { tenantAdminGuard } from './guards/tenant-admin.guard';


export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'select-tenant', component: TenantSelectComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },

  // Super-admin (platform-level tenant management)
  { path: 'admin/tenants', component: TenantsListComponent, canActivate: [superAdminGuard] },
  { path: 'admin/tenants/new', component: TenantFormComponent, canActivate: [superAdminGuard] },
  { path: 'admin/tenants/:id/edit', component: TenantFormComponent, canActivate: [superAdminGuard] },
  { path: 'admin/tenants/:id/applications', component: TenantApplicationsComponent, canActivate: [superAdminGuard] },
  { path: 'admin/applications', component: ApplicationsListComponent, canActivate: [superAdminGuard] },
  { path: 'admin/applications/new', component: ApplicationFormComponent, canActivate: [superAdminGuard] },
  { path: 'admin/applications/:id/edit', component: ApplicationFormComponent, canActivate: [superAdminGuard] },
  { path: 'admin/templates', component: AdminTemplatesComponent, canActivate: [superAdminGuard] },
  { path: 'admin/lookup', component: AdminLookupComponent, canActivate: [superAdminGuard] },

  // Tenant-admin (users + roles within the caller's tenant)
  { path: 'tenant-admin', redirectTo: 'tenant-admin/users', pathMatch: 'full' },
  { path: 'tenant-admin/users', component: UsersListComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/users/new', component: UserFormComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/users/:id/edit', component: UserFormComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/roles', component: RolesListComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/roles/new', component: RoleFormComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/roles/:id/edit', component: RoleFormComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/applications', component: TenantAdminApplicationsListComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/templates', component: TemplatesListComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/templates/new', component: TemplateEditorComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/templates/:id/edit', component: TemplateEditorComponent, canActivate: [tenantAdminGuard] },
  { path: 'tenant-admin/api-keys', component: ApiKeysListComponent, canActivate: [tenantAdminGuard] },
  { path: '**', redirectTo: 'login' }
];
