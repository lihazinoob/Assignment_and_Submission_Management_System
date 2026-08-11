import { apiClient } from "@/api/client"
import type { CreateClassRequest, SchoolClass, UpdateClassRequest } from "@/types/class"

export async function getClasses() {
  const { data } = await apiClient.get<SchoolClass[]>("/classes")
  return data
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
