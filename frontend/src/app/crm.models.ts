export interface CrmTask {
  id: string;
  name: string;
  status_id: string | null;
  department_id: string | null;
  assignee: string | null;
  deadline: string | null;
  description: string | null;
  cancel_reason: string | null;
}

export interface Department {
  id: string;
  name: string;
}

export interface Status {
  id: string;
  name: string;
}
