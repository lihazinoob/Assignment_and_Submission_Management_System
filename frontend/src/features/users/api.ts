import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type { UserRole } from "@/types/auth"
import type { User } from "@/types/user"

export interface UserFilters {
  role?: UserRole
  isActive?: boolean
  search?: string
}

export async function getUsersPaged(
  page: number,
  pageSize: number,
  filters: UserFilters = {}
) {
  const { data } = await apiClient.get<PagedResponse<User>>("/users", {
    params: { page, pageSize, ...filters },
  })
  return data
}

export async function getUsers(role?: UserRole) {
  const result = await getUsersPaged(1, 100, { role })
  return result.items
}

export function getTeachers() {
  return getUsers("Teacher")
}

export function getStudents() {
  return getUsers("Student")
}

export async function deactivateUser(id: string) {
  const { data } = await apiClient.post<User>(`/users/${id}/deactivate`)
  return data
}

export async function activateUser(id: string) {
  const { data } = await apiClient.post<User>(`/users/${id}/activate`)
  return data
}
