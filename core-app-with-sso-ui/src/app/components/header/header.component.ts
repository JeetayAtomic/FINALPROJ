import { Component, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ApplicationService } from '../../services/application.service';
import { ApplicationDto } from '../../models/app.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  showUserMenu = signal(false);
  showAppLauncher = signal(false);
  apps = signal<ApplicationDto[]>([]);
  loading = signal(true);

  isTenantAdmin = computed(() => this.auth.currentUser()?.role === 'Admin');
  tenantLogoUrl = computed(() => this.auth.currentUser()?.tenantLogoUrl || null);

  constructor(public auth: AuthService, private router: Router, private appService: ApplicationService) {}

  ngOnInit(): void {
    this.appService.getMyApplications().subscribe({
      next: (apps) => {
        this.apps.set(apps);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  toggleMenu(): void {
    this.showUserMenu.update(v => !v);
  }

  toggleAppLauncher(): void {
    this.showAppLauncher.update(v => !v);
  }

  openApp(app: ApplicationDto): void {
    this.appService.generateSSOToken(app.id).subscribe({
      next: (res) => {
        window.open(res.redirectUrl, '_blank');
        this.showAppLauncher.set(false);
      },
      error: () => {
        window.open(app.baseUrl, '_blank');
        this.showAppLauncher.set(false);
      }
    });
  }

  logout(): void {
    this.auth.logout();
  }

  openTenantAdmin(): void {
    this.showUserMenu.set(false);
    this.router.navigate(['/tenant-admin/users']);
  }

  getUserInitial(): string {
    const user = this.auth.currentUser();
    return user?.fullName?.charAt(0).toUpperCase() || 'U';
  }
}
