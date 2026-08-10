import { apiClient } from "@/api/client"
import type { ClassSubject, CreateClassSubjectRequest } from "@/types/class-subject"

export async function getClassSubjects() {
  const { data } = await apiClient.get<ClassSubject[]>("/class-subjects")
  return data
}

export async function createClassSubject(request: CreateClassSubjectRequest) {
  const { data } = await apiClient.post<ClassSubject>("/class-subjects", request)
  return data
}
