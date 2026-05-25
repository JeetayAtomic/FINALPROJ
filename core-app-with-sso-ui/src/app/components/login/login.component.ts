import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  error = signal('');
  loading = signal(false);

  constructor(private auth: AuthService, private router: Router) {
    if (auth.isLoggedIn()) {
      router.navigate([auth.isSuperAdmin() ? '/admin/tenants' : '/dashboard']);
    }
  }

  submit(): void {
    this.error.set('');
    this.loading.set(true);

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        if (res.isSuperAdmin) {
          this.router.navigate(['/admin/tenants']);
          return;
        }
        // Regular user: auto-select single tenant, otherwise show picker.
        if (res.tenants.length === 1) {
          this.auth.selectTenant({ tenantId: res.tenants[0].tenantId }).subscribe({
            next: () => this.router.navigate(['/dashboard']),
            error: (err) => {
              this.error.set(err.error?.message || 'Tenant selection failed');
              this.loading.set(false);
            }
          });
        } else {
          this.router.navigate(['/select-tenant']);
        }
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Invalid credentials');
        this.loading.set(false);
      }
    });
  }
}
