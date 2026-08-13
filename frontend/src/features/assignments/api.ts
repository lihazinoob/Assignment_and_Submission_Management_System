import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type {
  Assignment,
  AssignmentStatus,
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
} from "@/types/assignment"

export interface AssignmentFilters {
  status?: AssignmentStatus
  classSubjectId?: string
  search?: string
}

export async function getAssignmentsPaged(
  page: number,
  pageSize: number,
  filters: AssignmentFilters = {}
) {
  const { data } = await apiClient.get<PagedResponse<Assignment>>("/assignments", {
    params: { page, pageSize, ...filters },
  })
  return data
}

export async function getAssignments() {
  const result = await getAssignmentsPaged(1, 100)
  return result.items
}

export async function getAssignment(id: string) {
  const { data } = await apiClient.get<Assignment>(`/assignments/${id}`)
  return data
}

export async function createAssignment(request: CreateAssignmentRequest) {
  const { data } = await apiClient.post<Assignment>("/assignments", request)
  return data
}

export async function updateAssignment(
  id: string,
  request: UpdateAssignmentRequest
) {
  const { data } = await apiClient.put<Assignment>(
    `/assignments/${id}`,
    request
  )
  return data
}

export async function publishAssignment(id: string) {
  const { data } = await apiClient.post<Assignment>(
    `/assignments/${id}/publish`
  )
  return data
}

export async function deleteAssignment(id: string) {
  await apiClient.delete(`/assignments/${id}`)
}
