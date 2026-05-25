import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TenantSummary } from '../../models/app.models';

@Component({
  selector: 'app-tenant-select',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tenant-select.component.html',
  styleUrl: './tenant-select.component.css'
})
export class TenantSelectComponent {
  tenants: TenantSummary[] = [];
  error = signal('');
  loading = signal(false);

  constructor(private auth: AuthService, private router: Router) {
    this.tenants = auth.getPendingTenants();

    // No interim state — kick back to login.
    if (!auth.getInterimToken() || this.tenants.length === 0) {
      router.navigate(['/login']);
    }
  }

  pick(tenantId: number): void {
    if (this.loading()) return;
    this.error.set('');
    this.loading.set(true);

    this.auth.selectTenant({ tenantId }).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.error.set(err.error?.message || 'Tenant selection failed');
        this.loading.set(false);
      }
    });
  }

  cancel(): void {
    this.auth.logout();
  }

  initials(name: string): string {
    return (name || '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(s => s[0].toUpperCase())
      .join('') || '?';
  }
}
