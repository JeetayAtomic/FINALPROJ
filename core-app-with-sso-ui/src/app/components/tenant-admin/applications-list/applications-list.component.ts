import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TenantApplicationsService } from '../../../services/tenant-applications.service';
import { AuthService } from '../../../services/auth.service';
import { TenantApplicationDto } from '../../../models/app.models';

@Component({
  selector: 'app-tenant-applications-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './applications-list.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class TenantAdminApplicationsListComponent implements OnInit {
  rows = signal<TenantApplicationDto[]>([]);
  loading = signal(true);
  error = signal('');
  savingId = signal<number | null>(null);

  constructor(
    private svc: TenantApplicationsService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.svc.list().subscribe({
      next: (rows) => { this.rows.set(rows); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load applications.');
        this.loading.set(false);
      }
    });
  }

  toggle(row: TenantApplicationDto, active: boolean): void {
    this.savingId.set(row.applicationId);
    this.svc.toggle(row.applicationId, { isActive: active }).subscribe({
      next: (updated) => {
        this.rows.update(rs => rs.map(r => r.applicationId === updated.applicationId ? updated : r));
        this.savingId.set(null);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Save failed.');
        this.savingId.set(null);
      }
    });
  }

  goDashboard(): void { this.router.navigate(['/dashboard']); }
  logout(): void { this.auth.logout(); }
}
