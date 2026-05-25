import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TenantUsersService } from '../../../services/tenant-users.service';
import { AuthService } from '../../../services/auth.service';
import { TenantUserDto } from '../../../models/app.models';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './users-list.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class UsersListComponent implements OnInit {
  users = signal<TenantUserDto[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(
    private usersService: TenantUsersService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.usersService.list().subscribe({
      next: (u) => { this.users.set(u); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load users.');
        this.loading.set(false);
      }
    });
  }

  create(): void {
    this.router.navigate(['/tenant-admin/users/new']);
  }

  edit(u: TenantUserDto): void {
    this.router.navigate(['/tenant-admin/users', u.userId, 'edit']);
  }

  remove(u: TenantUserDto): void {
    if (!confirm(`Remove "${u.email}" from this tenant? The global account is kept.`)) return;
    this.usersService.remove(u.userId).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to remove user.')
    });
  }

  resetPassword(u: TenantUserDto): void {
    const pw = prompt(`New password for ${u.email} (min 6 chars):`);
    if (!pw) return;
    if (pw.length < 6) { this.error.set('Password must be at least 6 characters.'); return; }
    this.usersService.resetPassword(u.userId, { password: pw }).subscribe({
      next: () => alert('Password reset.'),
      error: (err) => this.error.set(err.error?.message || 'Failed to reset password.')
    });
  }

  goDashboard(): void { this.router.navigate(['/dashboard']); }
  logout(): void { this.auth.logout(); }
}
