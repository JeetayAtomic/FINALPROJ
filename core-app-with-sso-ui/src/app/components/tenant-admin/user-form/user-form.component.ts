import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TenantUsersService } from '../../../services/tenant-users.service';
import { RolesService } from '../../../services/roles.service';
import {
  CreateTenantUserDto,
  RoleDto,
  UpdateTenantUserDto
} from '../../../models/app.models';
import { forkJoin } from 'rxjs';

interface UserFormModel {
  fullName: string;
  email: string;
  password: string;
  roleName: string;
  isActive: boolean;
}

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-form.component.html',
  styleUrl: '../tenant-admin-shell.css'
})
export class UserFormComponent implements OnInit {
  isEdit = signal(false);
  userId = signal<number | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal('');
  roles = signal<RoleDto[]>([]);

  model: UserFormModel = {
    fullName: '',
    email: '',
    password: '',
    roleName: 'User',
    isActive: true
  };

  constructor(
    private users: TenantUsersService,
    private rolesService: RolesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEdit.set(!!id);
    if (id) this.userId.set(+id);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const roles$ = this.rolesService.list();
    const id = this.userId();

    if (id) {
      // Fetch users list and find this one (no single-get endpoint).
      forkJoin({ roles: roles$, users: this.users.list() }).subscribe({
        next: ({ roles, users }) => {
          this.roles.set(roles);
          const u = users.find(x => x.userId === id);
          if (u) {
            this.model = {
              fullName: u.fullName,
              email: u.email,
              password: '',
              roleName: u.roleName,
              isActive: u.isActive
            };
          } else {
            this.error.set('User not found in this tenant.');
          }
          this.loading.set(false);
        },
        error: (err) => { this.error.set(err.error?.message || 'Failed to load.'); this.loading.set(false); }
      });
    } else {
      roles$.subscribe({
        next: (r) => {
          this.roles.set(r);
          if (r.length > 0 && !this.model.roleName) this.model.roleName = r[0].name;
          this.loading.set(false);
        },
        error: (err) => { this.error.set(err.error?.message || 'Failed to load roles.'); this.loading.set(false); }
      });
    }
  }

  submit(): void {
    this.error.set('');
    this.submitting.set(true);

    const done = () => this.router.navigate(['/tenant-admin/users']);
    const fail = (err: any) => {
      this.error.set(err.error?.message || 'Save failed.');
      this.submitting.set(false);
    };

    if (this.isEdit()) {
      const dto: UpdateTenantUserDto = {
        fullName: this.model.fullName,
        roleName: this.model.roleName,
        isActive: this.model.isActive
      };
      this.users.update(this.userId()!, dto).subscribe({ next: done, error: fail });
    } else {
      const dto: CreateTenantUserDto = {
        fullName: this.model.fullName,
        email: this.model.email,
        password: this.model.password || null,
        roleName: this.model.roleName
      };
      this.users.create(dto).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void {
    this.router.navigate(['/tenant-admin/users']);
  }
}
