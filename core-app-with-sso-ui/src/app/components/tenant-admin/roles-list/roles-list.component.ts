import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { RolesService } from '../../../services/roles.service';
import { AuthService } from '../../../services/auth.service';
import { RoleDto } from '../../../models/app.models';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './roles-list.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class RolesListComponent implements OnInit {
  roles = signal<RoleDto[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(
    private rolesService: RolesService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.rolesService.list().subscribe({
      next: (r) => { this.roles.set(r); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load roles.');
        this.loading.set(false);
      }
    });
  }

  create(): void { this.router.navigate(['/tenant-admin/roles/new']); }
  edit(r: RoleDto): void { this.router.navigate(['/tenant-admin/roles', r.id, 'edit']); }

  remove(r: RoleDto): void {
    if (!confirm(`Delete role "${r.name}"? This is only allowed if no users have it.`)) return;
    this.rolesService.remove(r.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to delete role.')
    });
  }

  goDashboard(): void { this.router.navigate(['/dashboard']); }
  logout(): void { this.auth.logout(); }
}
