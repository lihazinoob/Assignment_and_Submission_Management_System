import { apiClient } from "@/api/client"
import type { CreateClassRequest, SchoolClass } from "@/types/class"

export async function getClasses() {
  const { data } = await apiClient.get<SchoolClass[]>("/classes")
  return data
}

export async function createClass(request: CreateClassRequest) {
  const { data } = await apiClient.post<SchoolClass>("/classes", request)
  return data
}
