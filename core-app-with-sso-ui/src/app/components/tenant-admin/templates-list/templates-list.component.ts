import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TemplatesService } from '../../../services/templates.service';
import { AuthService } from '../../../services/auth.service';
import { JsonTemplateSummary } from '../../../models/app.models';

@Component({
  selector: 'app-templates-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './templates-list.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class TemplatesListComponent implements OnInit {
  templates = signal<JsonTemplateSummary[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(
    private templatesService: TemplatesService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.templatesService.list().subscribe({
      next: (t) => { this.templates.set(t); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load templates.');
        this.loading.set(false);
      }
    });
  }

  create(): void { this.router.navigate(['/tenant-admin/templates/new']); }
  edit(t: JsonTemplateSummary): void { this.router.navigate(['/tenant-admin/templates', t.id, 'edit']); }

  remove(t: JsonTemplateSummary): void {
    if (!confirm(`Delete template "${t.name}"?`)) return;
    this.templatesService.remove(t.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to delete template.')
    });
  }

  goDashboard(): void { this.router.navigate(['/dashboard']); }
  logout(): void { this.auth.logout(); }
}
