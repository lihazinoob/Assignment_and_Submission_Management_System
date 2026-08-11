import { apiClient } from "@/api/client"
import type { UserRole } from "@/types/auth"
import type { User } from "@/types/user"

export async function getUsers(role?: UserRole) {
  const { data } = await apiClient.get<User[]>("/users", {
    params: role ? { role } : undefined,
  })
  return data
}

export function getTeachers() {
  return getUsers("Teacher")
}

export function getStudents() {
  return getUsers("Student")
}
