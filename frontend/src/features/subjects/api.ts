import { apiClient } from "@/api/client"
import type { CreateSubjectRequest, Subject } from "@/types/subject"

export async function getSubjects() {
  const { data } = await apiClient.get<Subject[]>("/subjects")
  return data
}

export async function createSubject(request: CreateSubjectRequest) {
  const { data } = await apiClient.post<Subject>("/subjects", request)
  return data
}
