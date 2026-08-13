import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type { ClassSubject, CreateClassSubjectRequest } from "@/types/class-subject"

export async function getClassSubjectsPaged(page: number, pageSize: number) {
  const { data } = await apiClient.get<PagedResponse<ClassSubject>>("/class-subjects", {
    params: { page, pageSize },
  })
  return data
}

export async function getClassSubjects() {
  const result = await getClassSubjectsPaged(1, 100)
  return result.items
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
