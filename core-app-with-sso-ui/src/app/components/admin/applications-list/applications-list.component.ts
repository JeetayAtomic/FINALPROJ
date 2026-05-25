import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApplicationsAdminService } from '../../../services/applications-admin.service';
import { AuthService } from '../../../services/auth.service';
import { ApplicationAdminDto } from '../../../models/app.models';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './applications-list.component.html',
  styleUrl: '../tenants-list/tenants-list.component.css'
})
export class ApplicationsListComponent implements OnInit {
  apps = signal<ApplicationAdminDto[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(
    private svc: ApplicationsAdminService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.svc.list().subscribe({
      next: (data) => { this.apps.set(data); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load applications.');
        this.loading.set(false);
      }
    });
  }

  create(): void { this.router.navigate(['/admin/applications/new']); }
  edit(a: ApplicationAdminDto): void { this.router.navigate(['/admin/applications', a.id, 'edit']); }

  deactivate(a: ApplicationAdminDto): void {
    if (!confirm(`Deactivate application "${a.name}"? It will be hidden from tenants until reactivated.`)) return;
    this.svc.deactivate(a.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to deactivate application.')
    });
  }

  logout(): void { this.auth.logout(); }
}
