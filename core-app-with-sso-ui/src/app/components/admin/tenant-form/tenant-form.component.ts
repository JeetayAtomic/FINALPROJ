import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TenantAdminService } from '../../../services/tenant-admin.service';
import { CreateTenantDto, UpdateTenantDto } from '../../../models/app.models';

interface TenantFormModel extends UpdateTenantDto {
  // Create-only fields. Holding them on the model keeps the form simple.
  initialAdminFullName?: string;
  initialAdminEmail?: string;
  initialAdminPassword?: string;
}

@Component({
  selector: 'app-tenant-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tenant-form.component.html',
  styleUrl: './tenant-form.component.css'
})
export class TenantFormComponent implements OnInit {
  isEdit = signal(false);
  id = signal<number | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal('');

  model: TenantFormModel = {
    name: '',
    dbName: '',
    clientCode: '',
    templateId: '',
    organizationId: '',
    clientFolderPath: '',
    rawFolderPath: '',
    processedFolderPath: '',
    errorFolderPath: '',
    publishFolderPath: '',
    jsonFolderPath: '',
    isActive: true,
    initialAdminFullName: '',
    initialAdminEmail: '',
    initialAdminPassword: ''
  };

  constructor(
    private tenantAdmin: TenantAdminService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = +idParam;
      this.id.set(id);
      this.isEdit.set(true);
      this.load(id);
    }
  }

  load(id: number): void {
    this.loading.set(true);
    this.tenantAdmin.get(id).subscribe({
      next: (t) => {
        this.model = {
          name: t.name,
          dbName: t.dbName,
          clientCode: t.clientCode,
          templateId: t.templateId ?? '',
          organizationId: t.organizationId ?? '',
          clientFolderPath: t.clientFolderPath ?? '',
          rawFolderPath: t.rawFolderPath ?? '',
          processedFolderPath: t.processedFolderPath ?? '',
          errorFolderPath: t.errorFolderPath ?? '',
          publishFolderPath: t.publishFolderPath ?? '',
          jsonFolderPath: t.jsonFolderPath ?? '',
          isActive: t.isActive
        };
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load tenant.');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    this.error.set('');
    this.submitting.set(true);

    const id = this.id();
    const done = () => this.router.navigate(['/admin/tenants']);
    const fail = (err: any) => {
      this.error.set(err.error?.message || 'Save failed.');
      this.submitting.set(false);
    };

    if (this.isEdit() && id != null) {
      const update: UpdateTenantDto = {
        name: this.model.name,
        dbName: this.model.dbName,
        clientCode: this.model.clientCode,
        templateId: this.model.templateId,
        organizationId: this.model.organizationId,
        clientFolderPath: this.model.clientFolderPath,
        rawFolderPath: this.model.rawFolderPath,
        processedFolderPath: this.model.processedFolderPath,
        errorFolderPath: this.model.errorFolderPath,
        publishFolderPath: this.model.publishFolderPath,
        jsonFolderPath: this.model.jsonFolderPath,
        isActive: this.model.isActive
      };
      this.tenantAdmin.update(id, update).subscribe({ next: done, error: fail });
    } else {
      const create: CreateTenantDto = {
        name: this.model.name,
        dbName: this.model.dbName,
        clientCode: this.model.clientCode,
        templateId: this.model.templateId,
        organizationId: this.model.organizationId,
        clientFolderPath: this.model.clientFolderPath,
        rawFolderPath: this.model.rawFolderPath,
        processedFolderPath: this.model.processedFolderPath,
        errorFolderPath: this.model.errorFolderPath,
        publishFolderPath: this.model.publishFolderPath,
        jsonFolderPath: this.model.jsonFolderPath,
        initialAdminFullName: this.model.initialAdminFullName ?? '',
        initialAdminEmail: this.model.initialAdminEmail ?? '',
        initialAdminPassword: this.model.initialAdminPassword ?? ''
      };
      this.tenantAdmin.create(create).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void {
    this.router.navigate(['/admin/tenants']);
  }

  suggestDbName(): void {
    if (this.isEdit()) return;
    const slug = (this.model.name || '')
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '_')
      .replace(/^_+|_+$/g, '');
    if (slug) this.model.dbName = `CoreAppwithSSO_Tenant_${slug}`;
  }
}
