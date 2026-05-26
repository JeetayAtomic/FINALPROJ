import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from '../header/header.component';
import { firstValueFrom } from 'rxjs';
import { ApplicationService } from '../../services/application.service';
import { AuthService } from '../../services/auth.service';
import { openOrFocusAppTab } from '../../services/app-tab-launcher';
import { ApplicationDto } from '../../models/app.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, HeaderComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  apps = signal<ApplicationDto[]>([]);
  loading = signal(true);
  tenantLogoUrl = computed(() => this.auth.currentUser()?.tenantLogoUrl || null);
  tenantName = computed(() => this.auth.currentUser()?.tenantName || '');

  constructor(private appService: ApplicationService, public auth: AuthService) {}

  ngOnInit(): void {
    this.appService.getMyApplications().subscribe({
      next: (apps) => {
        this.apps.set(apps);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openApp(app: ApplicationDto): void {
    openOrFocusAppTab(app, async () => {
      const res = await firstValueFrom(this.appService.generateSSOToken(app.id));
      return res.redirectUrl;
    });
  }
}
