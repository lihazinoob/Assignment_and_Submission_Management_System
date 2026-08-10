import { apiClient } from "@/api/client"
import type {
  Assignment,
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
} from "@/types/assignment"

export async function getAssignments() {
  const { data } = await apiClient.get<Assignment[]>("/assignments")
  return data
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
