import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CrmTask, Department, Status } from './crm.models';
import { CrmApiService } from './crm-api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly api = inject(CrmApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly taskForm = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    statusId: [''],
    departmentId: [''],
    assignee: [''],
    deadline: [''],
    description: [''],
    cancelReason: ['', Validators.maxLength(250)]
  });

  tasks: CrmTask[] = [];
  departments: Department[] = [];
  statuses: Status[] = [];
  loading = true;
  saving = false;
  formOpen = false;
  editingId: string | null = null;
  pageError = '';
  formError = '';
  successMessage = '';
  searchText = '';
  statusFilter = '';

  ngOnInit(): void {
    this.loadData();
    this.taskForm.controls.statusId.valueChanges.subscribe(() => this.updateCancelReasonValidation());
  }

  get filteredTasks(): CrmTask[] {
    const query = this.searchText.trim().toLocaleLowerCase();
    return this.tasks.filter((task) => {
      const matchesStatus = !this.statusFilter || task.status_id === this.statusFilter;
      const searchTarget = [
        task.name,
        task.description,
        task.assignee,
        this.departmentName(task.department_id),
        this.statusName(task.status_id)
      ].join(' ').toLocaleLowerCase();
      return matchesStatus && (!query || searchTarget.includes(query));
    });
  }

  get isCancelled(): boolean {
    return this.statuses.find((status) => status.id === this.taskForm.controls.statusId.value)?.name === 'Скасовано';
  }

  countByStatus(name: string): number {
    const statusIds = new Set(this.statuses.filter((status) => status.name === name).map((status) => status.id));
    return this.tasks.filter((task) => task.status_id !== null && statusIds.has(task.status_id)).length;
  }

  statusName(id: string | null): string {
    return this.statuses.find((status) => status.id === id)?.name ?? 'Без статусу';
  }

  departmentName(id: string | null): string {
    return this.departments.find((department) => department.id === id)?.name ?? '—';
  }

  statusClass(id: string | null): string {
    const name = this.statusName(id);
    if (name === 'В роботі') return 'status-progress';
    if (name === 'Виконано') return 'status-done';
    if (name === 'Скасовано') return 'status-cancelled';
    return 'status-new';
  }

  formatDate(value: string | null): string {
    if (!value) return '—';
    const [year, month, day] = value.split('-').map(Number);
    return new Intl.DateTimeFormat('uk-UA', { day: 'numeric', month: 'short', year: 'numeric' })
      .format(new Date(year, month - 1, day));
  }

  isOverdue(deadline: string | null, statusId: string | null): boolean {
    if (!deadline || this.statusName(statusId) === 'Виконано' || this.statusName(statusId) === 'Скасовано') {
      return false;
    }
    return deadline < new Date().toISOString().slice(0, 10);
  }

  setSearch(event: Event): void {
    this.searchText = (event.target as HTMLInputElement).value;
  }

  setStatusFilter(event: Event): void {
    this.statusFilter = (event.target as HTMLSelectElement).value;
  }

  startCreate(): void {
    this.editingId = null;
    this.formError = '';
    this.taskForm.reset({
      name: '',
      statusId: '',
      departmentId: '',
      assignee: '',
      deadline: '',
      description: '',
      cancelReason: ''
    });
    this.updateCancelReasonValidation();
    this.formOpen = true;
  }

  startEdit(task: CrmTask): void {
    this.editingId = task.id;
    this.formError = '';
    this.taskForm.reset({
      name: task.name ?? '',
      statusId: task.status_id ?? '',
      departmentId: task.department_id ?? '',
      assignee: task.assignee ?? '',
      deadline: task.deadline ?? '',
      description: task.description ?? '',
      cancelReason: task.cancel_reason ?? ''
    });
    this.updateCancelReasonValidation();
    this.formOpen = true;
  }

  closeForm(): void {
    if (!this.saving) {
      this.formOpen = false;
    }
  }

  isInvalid(controlName: 'name' | 'cancelReason'): boolean {
    const control = this.taskForm.controls[controlName];
    return control.invalid && (control.touched || control.dirty || !!this.formError);
  }

  saveTask(): void {
    this.updateCancelReasonValidation();
    this.taskForm.markAllAsTouched();
    this.formError = '';
    if (this.taskForm.invalid) {
      return;
    }

    const form = this.taskForm.getRawValue();
    const task: CrmTask = {
      id: this.editingId ?? '',
      name: form.name.trim(),
      status_id: form.statusId || null,
      department_id: form.departmentId || null,
      assignee: form.assignee.trim() || null,
      deadline: form.deadline || null,
      description: form.description.trim() || null,
      cancel_reason: form.cancelReason.trim() || null
    };

    this.saving = true;
    const request = this.editingId
      ? this.api.updateTask(this.editingId, task)
      : this.api.createTask(task);
    request.subscribe({
      next: () => {
        this.saving = false;
        this.formOpen = false;
        this.successMessage = this.editingId ? 'Зміни збережено.' : 'Задачу створено.';
        this.loadData(false);
      },
      error: (error: unknown) => {
        this.saving = false;
        this.formError = this.readError(error);
      }
    });
  }

  deleteTask(task: CrmTask): void {
    if (!window.confirm(`Видалити задачу «${task.name || 'Без назви'}»?`)) {
      return;
    }
    this.pageError = '';
    this.successMessage = '';
    this.api.deleteTask(task.id).subscribe({
      next: () => {
        this.successMessage = 'Задачу видалено.';
        this.loadData(false);
      },
      error: (error: unknown) => this.pageError = this.readError(error)
    });
  }

  private loadData(showLoading = true): void {
    if (showLoading) this.loading = true;
    this.pageError = '';
    forkJoin({
      tasks: this.api.getTasks(),
      departments: this.api.getDepartments(),
      statuses: this.api.getStatuses()
    }).subscribe({
      next: (data) => {
        this.tasks = data.tasks;
        this.departments = data.departments;
        this.statuses = data.statuses;
        this.loading = false;
        this.updateCancelReasonValidation();
      },
      error: (error: unknown) => {
        this.loading = false;
        this.pageError = `${this.readError(error)} Переконайтеся, що API запущено.`;
      }
    });
  }

  private updateCancelReasonValidation(): void {
    const control = this.taskForm.controls.cancelReason;
    if (this.isCancelled) {
      control.setValidators([Validators.required, Validators.maxLength(250)]);
    } else {
      control.setValidators([Validators.maxLength(250)]);
    }
    control.updateValueAndValidity({ emitEvent: false });
  }

  private readError(error: unknown): string {
    const response = error as { error?: { error?: string; title?: string; message?: string } };
    return response.error?.error
      ?? response.error?.title
      ?? response.error?.message
      ?? 'Не вдалося виконати запит до API.';
  }
}
