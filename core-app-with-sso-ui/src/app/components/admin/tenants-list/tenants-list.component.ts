import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TenantAdminService } from '../../../services/tenant-admin.service';
import { AuthService } from '../../../services/auth.service';
import { SeedTenantUserDto, TenantDto } from '../../../models/app.models';

@Component({
  selector: 'app-tenants-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './tenants-list.component.html',
  styleUrl: './tenants-list.component.css'
})
export class TenantsListComponent implements OnInit {
  tenants = signal<TenantDto[]>([]);
  loading = signal(true);
  error = signal('');

  // Seed-admin dialog state
  seedOpen = signal(false);
  seedTarget = signal<TenantDto | null>(null);
  seedSubmitting = signal(false);
  seedError = signal('');
  seedModel: SeedTenantUserDto = { fullName: '', email: '', password: '', roleName: 'Admin' };

  constructor(
    private tenantAdmin: TenantAdminService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.tenantAdmin.list().subscribe({
      next: (data) => {
        this.tenants.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load tenants.');
        this.loading.set(false);
      }
    });
  }

  create(): void { this.router.navigate(['/admin/tenants/new']); }
  edit(t: TenantDto): void { this.router.navigate(['/admin/tenants', t.id, 'edit']); }

  deactivate(t: TenantDto): void {
    if (!confirm(`Deactivate tenant "${t.name}"? They won't be able to log in.`)) return;
    this.tenantAdmin.deactivate(t.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to deactivate tenant.')
    });
  }

  openSeed(t: TenantDto): void {
    this.seedError.set('');
    this.seedModel = { fullName: '', email: '', password: '', roleName: 'Admin' };
    this.seedTarget.set(t);
    this.seedOpen.set(true);
  }

  closeSeed(): void {
    this.seedOpen.set(false);
    this.seedTarget.set(null);
  }

  submitSeed(): void {
    const t = this.seedTarget();
    if (!t) return;
    this.seedError.set('');
    this.seedSubmitting.set(true);
    this.tenantAdmin.seedUser(t.id, this.seedModel).subscribe({
      next: () => {
        this.seedSubmitting.set(false);
        this.closeSeed();
      },
      error: (err) => {
        this.seedError.set(err.error?.message || 'Failed to create user.');
        this.seedSubmitting.set(false);
      }
    });
  }

  logout(): void { this.auth.logout(); }

  initials(name: string): string {
    return (name || '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(s => s[0].toUpperCase())
      .join('') || '?';
  }
}
