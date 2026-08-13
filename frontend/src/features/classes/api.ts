import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type { CreateClassRequest, SchoolClass, UpdateClassRequest } from "@/types/class"

export async function getClassesPaged(page: number, pageSize: number) {
  const { data } = await apiClient.get<PagedResponse<SchoolClass>>("/classes", {
    params: { page, pageSize },
  })
  return data
}

export async function getClasses() {
  const result = await getClassesPaged(1, 100)
  return result.items
}

export async function createClass(request: CreateClassRequest) {
  const { data } = await apiClient.post<SchoolClass>("/classes", request)
  return data
}

export async function updateClass(id: string, request: UpdateClassRequest) {
  const { data } = await apiClient.put<SchoolClass>(`/classes/${id}`, request)
  return data
}

export async function deactivateClass(id: string) {
  const { data } = await apiClient.post<SchoolClass>(`/classes/${id}/deactivate`)
  return data
}

export async function activateClass(id: string) {
  const { data } = await apiClient.post<SchoolClass>(`/classes/${id}/activate`)
  return data
}

export async function deleteClass(id: string) {
  await apiClient.delete(`/classes/${id}`)
}
