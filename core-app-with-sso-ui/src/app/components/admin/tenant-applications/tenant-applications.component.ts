import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApplicationsAdminService } from '../../../services/applications-admin.service';
import { TenantAdminService } from '../../../services/tenant-admin.service';
import { TenantApplicationDto, TenantDto } from '../../../models/app.models';

@Component({
  selector: 'app-tenant-applications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tenant-applications.component.html',
  styleUrl: '../tenants-list/tenants-list.component.css'
})
export class TenantApplicationsComponent implements OnInit {
  tenant = signal<TenantDto | null>(null);
  rows = signal<TenantApplicationDto[]>([]);
  loading = signal(true);
  error = signal('');
  savingId = signal<number | null>(null);

  tenantId = 0;

  constructor(
    private svc: ApplicationsAdminService,
    private tenantSvc: TenantAdminService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.tenantId = +(this.route.snapshot.paramMap.get('id') || 0);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.tenantSvc.get(this.tenantId).subscribe({
      next: (t) => this.tenant.set(t),
      error: () => {}
    });
    this.svc.listTenantApps(this.tenantId).subscribe({
      next: (rows) => { this.rows.set(rows); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load applications.');
        this.loading.set(false);
      }
    });
  }

  toggleSubscribed(row: TenantApplicationDto, subscribed: boolean): void {
    this.savingId.set(row.applicationId);
    if (subscribed) {
      this.svc.upsertTenantApp(this.tenantId, row.applicationId, { isActive: true }).subscribe({
        next: (updated) => { this.replace(updated); this.savingId.set(null); },
        error: (err) => { this.error.set(err.error?.message || 'Save failed.'); this.savingId.set(null); }
      });
    } else {
      this.svc.removeTenantApp(this.tenantId, row.applicationId).subscribe({
        next: () => {
          this.rows.update(rs => rs.map(r => r.applicationId === row.applicationId
            ? { ...r, isSubscribed: false, subscriptionActive: false, assignedAt: null }
            : r));
          this.savingId.set(null);
        },
        error: (err) => { this.error.set(err.error?.message || 'Remove failed.'); this.savingId.set(null); }
      });
    }
  }

  toggleActive(row: TenantApplicationDto, active: boolean): void {
    this.savingId.set(row.applicationId);
    this.svc.upsertTenantApp(this.tenantId, row.applicationId, { isActive: active }).subscribe({
      next: (updated) => { this.replace(updated); this.savingId.set(null); },
      error: (err) => { this.error.set(err.error?.message || 'Save failed.'); this.savingId.set(null); }
    });
  }

  private replace(updated: TenantApplicationDto): void {
    this.rows.update(rs => rs.map(r => r.applicationId === updated.applicationId ? updated : r));
  }

  back(): void { this.router.navigate(['/admin/tenants']); }
}
