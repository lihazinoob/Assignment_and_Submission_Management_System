import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type { CreateSubjectRequest, Subject } from "@/types/subject"

export async function getSubjectsPaged(page: number, pageSize: number) {
  const { data } = await apiClient.get<PagedResponse<Subject>>("/subjects", {
    params: { page, pageSize },
  })
  return data
}

export async function getSubjects() {
  const result = await getSubjectsPaged(1, 100)
  return result.items
}

export async function createSubject(request: CreateSubjectRequest) {
  const { data } = await apiClient.post<Subject>("/subjects", request)
  return data
}
