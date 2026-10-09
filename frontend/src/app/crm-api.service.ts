import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CrmTask, Department, Status } from './crm.models';

@Injectable({ providedIn: 'root' })
export class CrmApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '';

  getTasks(): Observable<CrmTask[]> {
    return this.http.get<CrmTask[]>(`${this.apiUrl}/CrmTask`);
  }

  createTask(task: CrmTask): Observable<unknown> {
    const { id: _id, ...body } = task;
    return this.http.post<unknown>(`${this.apiUrl}/CrmTask`, body);
  }

  updateTask(id: string, task: CrmTask): Observable<unknown> {
    return this.http.put<unknown>(`${this.apiUrl}/CrmTask/${id}`, task);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/CrmTask/${id}`);
  }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/Department`);
  }

  getStatuses(): Observable<Status[]> {
    return this.http.get<Status[]>(`${this.apiUrl}/Status`);
  }
}
