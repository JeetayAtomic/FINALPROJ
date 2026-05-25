import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AdminTemplatesService } from '../../../services/admin-templates.service';
import { AuthService } from '../../../services/auth.service';
import { AdminTemplateRow } from '../../../models/app.models';

@Component({
  selector: 'app-admin-templates',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './admin-templates.component.html',
  styleUrl: '../tenants-list/tenants-list.component.css'
})
export class AdminTemplatesComponent implements OnInit {
  rows = signal<AdminTemplateRow[]>([]);
  loading = signal(true);
  error = signal('');

  /** Bound to the dropdown via ngModel. */
  filterValue = '';
  tenantFilter = signal<string>('');

  selected = signal<AdminTemplateRow | null>(null);
  selectedJson = signal<string>('');
  selectedLoading = signal(false);
  selectedError = signal('');

  /** Distinct tenants for the filter dropdown. */
  tenants = computed(() => {
    const seen = new Map<number, string>();
    for (const r of this.rows()) {
      if (!seen.has(r.tenantId)) seen.set(r.tenantId, r.tenantName);
    }
    return Array.from(seen.entries())
      .map(([id, name]) => ({ id, name }))
      .sort((a, b) => a.name.localeCompare(b.name));
  });

  /** Per-tenant counts for a quick summary at the top. */
  tenantCounts = computed(() => {
    const counts = new Map<string, number>();
    for (const r of this.rows()) {
      counts.set(r.tenantName, (counts.get(r.tenantName) ?? 0) + 1);
    }
    return Array.from(counts.entries())
      .map(([name, count]) => ({ name, count }))
      .sort((a, b) => b.count - a.count);
  });

  filtered = computed(() => {
    const f = this.tenantFilter();
    if (!f) return this.rows();
    return this.rows().filter(r => String(r.tenantId) === f);
  });

  constructor(
    private adminTemplates: AdminTemplatesService,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.adminTemplates.listAll().subscribe({
      next: (data) => { this.rows.set(data); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load templates.');
        this.loading.set(false);
      }
    });
  }

  /** Fetches the full JSON body via the lookup endpoint (list endpoint omits it). */
  view(r: AdminTemplateRow): void {
    this.selected.set(r);
    this.selectedJson.set('');
    this.selectedError.set('');
    this.selectedLoading.set(true);
    this.adminTemplates.lookup(r.clientCode, r.templateName).subscribe({
      next: (res) => {
        this.selectedJson.set(this.pretty(res.template.jsonContent));
        this.selectedLoading.set(false);
      },
      error: (err) => {
        this.selectedError.set(err.error?.message || 'Failed to load template content.');
        this.selectedLoading.set(false);
      }
    });
  }

  closePanel(): void {
    this.selected.set(null);
    this.selectedJson.set('');
    this.selectedError.set('');
  }

  logout(): void { this.auth.logout(); }

  private pretty(json: string): string {
    try { return JSON.stringify(JSON.parse(json), null, 2); }
    catch { return json; }
  }
}
