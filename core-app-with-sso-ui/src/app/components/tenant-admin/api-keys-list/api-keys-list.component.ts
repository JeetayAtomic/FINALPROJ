import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApiKeysService } from '../../../services/api-keys.service';
import { AuthService } from '../../../services/auth.service';
import { ApiKeySummary, CreateApiKeyResponse } from '../../../models/app.models';

@Component({
  selector: 'app-api-keys-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './api-keys-list.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class ApiKeysListComponent implements OnInit {
  keys = signal<ApiKeySummary[]>([]);
  loading = signal(true);
  error = signal('');

  showCreate = signal(false);
  submitting = signal(false);

  newName = '';
  newScopesCsv = '';
  newExpiresAt = '';

  justCreated = signal<CreateApiKeyResponse | null>(null);
  copied = signal(false);

  constructor(
    private service: ApiKeysService,
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.list().subscribe({
      next: (k) => { this.keys.set(k); this.loading.set(false); },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load API keys.');
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.newName = '';
    this.newScopesCsv = '';
    this.newExpiresAt = '';
    this.showCreate.set(true);
  }

  cancelCreate(): void {
    this.showCreate.set(false);
  }

  submitCreate(): void {
    if (!this.newName.trim() || this.submitting()) return;
    this.submitting.set(true);
    this.error.set('');

    const scopes = this.newScopesCsv
      .split(',')
      .map((s) => s.trim())
      .filter((s) => s.length > 0);

    this.service
      .create({
        name: this.newName.trim(),
        scopes: scopes.length ? scopes : null,
        expiresAt: this.newExpiresAt ? new Date(this.newExpiresAt).toISOString() : null
      })
      .subscribe({
        next: (resp) => {
          this.submitting.set(false);
          this.showCreate.set(false);
          this.justCreated.set(resp);
          this.copied.set(false);
          this.load();
        },
        error: (err) => {
          this.error.set(err.error?.message || 'Failed to create API key.');
          this.submitting.set(false);
        }
      });
  }

  copyFullKey(): void {
    const key = this.justCreated()?.fullKey;
    if (!key) return;
    navigator.clipboard.writeText(key).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    });
  }

  dismissJustCreated(): void {
    if (!confirm('Close this dialog? The full key will not be shown again. Make sure you have copied it.')) return;
    this.justCreated.set(null);
  }

  revoke(k: ApiKeySummary): void {
    if (!confirm(`Revoke "${k.name}"? Vendors using this key will lose access immediately.`)) return;
    this.service.revoke(k.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to revoke key.')
    });
  }

  remove(k: ApiKeySummary): void {
    if (!confirm(`Permanently delete "${k.name}"? This cannot be undone.`)) return;
    this.service.remove(k.id).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err.error?.message || 'Failed to delete key.')
    });
  }

  goDashboard(): void { this.router.navigate(['/dashboard']); }
  logout(): void { this.auth.logout(); }
}
