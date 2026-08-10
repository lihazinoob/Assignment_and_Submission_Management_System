import { apiClient } from "@/api/client"
import type {
  CreateSubmissionRequest,
  GradeSubmissionRequest,
  Submission,
  UpdateSubmissionRequest,
} from "@/types/submission"

export async function getSubmissions(assignmentId?: string) {
  const { data } = await apiClient.get<Submission[]>("/submissions", {
    params: assignmentId ? { assignmentId } : undefined,
  })
  return data
}

export async function getSubmission(id: string) {
  const { data } = await apiClient.get<Submission>(`/submissions/${id}`)
  return data
}

export async function createSubmission(request: CreateSubmissionRequest) {
  const { data } = await apiClient.post<Submission>("/submissions", request)
  return data
}

export async function updateSubmission(
  id: string,
  request: UpdateSubmissionRequest
) {
  const { data } = await apiClient.put<Submission>(
    `/submissions/${id}`,
    request
  )
  return data
}

export async function gradeSubmission(
  id: string,
  request: GradeSubmissionRequest
) {
  const { data } = await apiClient.post<Submission>(
    `/submissions/${id}/grade`,
    request
  )
  return data
}
