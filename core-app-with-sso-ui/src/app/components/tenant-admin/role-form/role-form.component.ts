import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { RolesService } from '../../../services/roles.service';
import {
  ApplicationDto,
  CreateRoleDto,
  RoleDto,
  UpdateRoleDto
} from '../../../models/app.models';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './role-form.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class RoleFormComponent implements OnInit {
  isEdit = signal(false);
  id = signal<number | null>(null);
  role = signal<RoleDto | null>(null);
  apps = signal<ApplicationDto[]>([]);
  selected = signal<Set<number>>(new Set());
  loading = signal(false);
  submitting = signal(false);
  error = signal('');

  name = '';
  description = '';

  constructor(
    private rolesService: RolesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit.set(true);
      this.id.set(+idParam);
    }
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const role$ = this.id() != null ? this.rolesService.get(this.id()!) : of(null);
    forkJoin({ apps: this.rolesService.tenantCatalog(), role: role$ }).subscribe({
      next: ({ apps, role }) => {
        this.apps.set(apps);
        if (role) {
          this.role.set(role);
          this.name = role.name;
          this.description = role.description ?? '';
          this.selected.set(new Set(role.applicationIds));
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load.');
        this.loading.set(false);
      }
    });
  }

  isSelected(appId: number): boolean {
    return this.selected().has(appId);
  }

  toggle(appId: number): void {
    // System roles (Admin, User) can't be renamed, but their app access IS editable.
    const next = new Set(this.selected());
    if (next.has(appId)) next.delete(appId); else next.add(appId);
    this.selected.set(next);
  }

  isSystemRenameLocked(): boolean {
    return this.role()?.isSystem === true;
  }

  submit(): void {
    this.error.set('');
    this.submitting.set(true);

    const payload: CreateRoleDto = {
      name: this.name,
      description: this.description || null,
      applicationIds: Array.from(this.selected())
    };
    const done = () => this.router.navigate(['/tenant-admin/roles']);
    const fail = (err: any) => {
      this.error.set(err.error?.message || 'Save failed.');
      this.submitting.set(false);
    };

    if (this.isEdit() && this.id() != null) {
      this.rolesService.update(this.id()!, payload as UpdateRoleDto).subscribe({ next: done, error: fail });
    } else {
      this.rolesService.create(payload).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void {
    this.router.navigate(['/tenant-admin/roles']);
  }
}
