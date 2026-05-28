import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ApplicationService } from '../../services/application.service';

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

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private appService: ApplicationService
  ) {
    // Sample apps redirect here with ?returnUrl=<app-root> on logout. Stash it so
    // it survives the optional /select-tenant hop and the success handler below.
    this.appService.captureReturnUrl(this.route.snapshot.queryParamMap.get('returnUrl'));

    if (auth.isLoggedIn()) {
      this.appService.redirectAfterLogin(auth.isSuperAdmin());
    }
  }

  submit(): void {
    this.error.set('');
    this.loading.set(true);

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        if (res.isSuperAdmin) {
          this.appService.redirectAfterLogin(true);
          return;
        }
        // Regular user: auto-select single tenant, otherwise show picker.
        if (res.tenants.length === 1) {
          this.auth.selectTenant({ tenantId: res.tenants[0].tenantId }).subscribe({
            next: () => this.appService.redirectAfterLogin(false),
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
