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

export async function deactivateClassSubject(id: string) {
  const { data } = await apiClient.post<ClassSubject>(`/class-subjects/${id}/deactivate`)
  return data
}

export async function activateClassSubject(id: string) {
  const { data } = await apiClient.post<ClassSubject>(`/class-subjects/${id}/activate`)
  return data
}

export async function deleteClassSubject(id: string) {
  await apiClient.delete(`/class-subjects/${id}`)
}
