import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AdminTemplatesService } from '../../../services/admin-templates.service';
import { AuthService } from '../../../services/auth.service';
import { AdminLookupResponse } from '../../../models/app.models';

@Component({
  selector: 'app-admin-lookup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './admin-lookup.component.html',
  styleUrl: '../tenants-list/tenants-list.component.css'
})
export class AdminLookupComponent {
  clientCode = '';
  templateName = '';

  result = signal<AdminLookupResponse | null>(null);
  prettyJson = signal<string>('');
  loading = signal(false);
  error = signal('');
  notFound = signal(false);

  constructor(
    private adminTemplates: AdminTemplatesService,
    public auth: AuthService
  ) {}

  search(): void {
    this.error.set('');
    this.notFound.set(false);
    this.result.set(null);
    this.prettyJson.set('');

    if (!this.clientCode.trim() || !this.templateName.trim()) {
      this.error.set('Both Client Code and Template Name are required.');
      return;
    }

    this.loading.set(true);
    this.adminTemplates.lookup(this.clientCode.trim(), this.templateName.trim()).subscribe({
      next: (res) => {
        this.result.set(res);
        this.prettyJson.set(this.pretty(res.template.jsonContent));
        this.loading.set(false);
      },
      error: (err) => {
        if (err.status === 404) {
          this.notFound.set(true);
          this.error.set(err.error?.message || 'Not found.');
        } else {
          this.error.set(err.error?.message || 'Lookup failed.');
        }
        this.loading.set(false);
      }
    });
  }

  logout(): void { this.auth.logout(); }

  private pretty(json: string): string {
    try { return JSON.stringify(JSON.parse(json), null, 2); }
    catch { return json; }
  }
}
