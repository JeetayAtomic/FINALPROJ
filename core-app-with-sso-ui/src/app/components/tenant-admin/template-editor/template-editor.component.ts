import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TemplatesService } from '../../../services/templates.service';
import { CreateJsonTemplateDto, UpdateJsonTemplateDto } from '../../../models/app.models';

@Component({
  selector: 'app-template-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './template-editor.component.html',
  styleUrl: './template-editor.component.css'
})
export class TemplateEditorComponent implements OnInit {
  isEdit = signal(false);
  id = signal<number | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal('');
  jsonError = signal('');
  version = signal(1);

  name = '';
  description = '';
  jsonContent = '{\n  \n}';

  constructor(
    private templates: TemplatesService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id.set(+idParam);
      this.isEdit.set(true);
      this.load(+idParam);
    }
  }

  load(id: number): void {
    this.loading.set(true);
    this.templates.get(id).subscribe({
      next: (t) => {
        this.name = t.name;
        this.description = t.description ?? '';
        this.jsonContent = t.jsonContent;
        this.version.set(t.version);
        this.prettyPrint();
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load template.');
        this.loading.set(false);
      }
    });
  }

  prettyPrint(): void {
    try {
      const obj = JSON.parse(this.jsonContent);
      this.jsonContent = JSON.stringify(obj, null, 2);
      this.jsonError.set('');
    } catch (e: any) {
      this.jsonError.set(e.message || 'Invalid JSON');
    }
  }

  validateJson(): void {
    try {
      JSON.parse(this.jsonContent);
      this.jsonError.set('');
    } catch (e: any) {
      this.jsonError.set(e.message || 'Invalid JSON');
    }
  }

  submit(): void {
    this.error.set('');
    this.validateJson();
    if (this.jsonError()) return;

    this.submitting.set(true);
    const done = () => this.router.navigate(['/tenant-admin/templates']);
    const fail = (err: any) => {
      this.error.set(err.error?.message || 'Save failed.');
      this.submitting.set(false);
    };

    if (this.isEdit() && this.id() != null) {
      const dto: UpdateJsonTemplateDto = {
        name: this.name,
        description: this.description || null,
        jsonContent: this.jsonContent
      };
      this.templates.update(this.id()!, dto).subscribe({ next: done, error: fail });
    } else {
      const dto: CreateJsonTemplateDto = {
        name: this.name,
        description: this.description || null,
        jsonContent: this.jsonContent
      };
      this.templates.create(dto).subscribe({ next: done, error: fail });
    }
  }

  cancel(): void {
    this.router.navigate(['/tenant-admin/templates']);
  }
}
