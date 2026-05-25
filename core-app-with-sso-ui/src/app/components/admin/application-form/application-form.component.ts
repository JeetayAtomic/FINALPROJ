import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApplicationsAdminService } from '../../../services/applications-admin.service';
import { CreateApplicationDto, UpdateApplicationDto } from '../../../models/app.models';

interface ApplicationFormModel extends UpdateApplicationDto {}

@Component({
  selector: 'app-application-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './application-form.component.html',
  styleUrl: '../tenant-form/tenant-form.component.css'
})
export class ApplicationFormComponent implements OnInit {
  isEdit = signal(false);
  id = signal<number | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal('');

  model: ApplicationFormModel = {
    name: '',
    description: '',
    baseUrl: '',
    iconName: 'apps',
    iconColor: '#4285F4',
    displayOrder: 0,
    isActive: true
  };

  constructor(
    private svc: ApplicationsAdminService,
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
    this.svc.get(id).subscribe({
      next: (a) => {
        this.model = {
          name: a.name,
          description: a.description,
          baseUrl: a.baseUrl,
          iconName: a.iconName,
          iconColor: a.iconColor,
          displayOrder: a.displayOrder,
          isActive: a.isActive
        };
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load application.');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    this.error.set('');
    this.submitting.set(true);
    const done = () => this.router.navigate(['/admin/applications']);
    const fail = (err: any) => {
      this.error.set(err.error?.message || 'Save failed.');
      this.submitting.set(false);
    };

    const id = this.id();
    if (this.isEdit() && id != null) {
      this.svc.update(id, { ...this.model }).subscribe({ next: done, error: fail });
    } else {
      const create: CreateApplicationDto = {
        name: this.model.name,
        description: this.model.description,
        baseUrl: this.model.baseUrl,
        iconName: this.model.iconName,
        iconColor: this.model.iconColor,
        displayOrder: this.model.displayOrder
      };
      this.svc.create(create).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void { this.router.navigate(['/admin/applications']); }
}
