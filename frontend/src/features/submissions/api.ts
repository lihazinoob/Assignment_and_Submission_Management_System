import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type {
  CreateSubmissionRequest,
  GradeSubmissionRequest,
  Submission,
  SubmissionStatus,
  UpdateSubmissionRequest,
} from "@/types/submission"

export interface SubmissionFilters {
  assignmentId?: string
  status?: SubmissionStatus
}

export async function getSubmissionsPaged(
  page: number,
  pageSize: number,
  filters: SubmissionFilters = {}
) {
  const { data } = await apiClient.get<PagedResponse<Submission>>("/submissions", {
    params: { page, pageSize, ...filters },
  })
  return data
}

export async function getSubmissions(assignmentId?: string) {
  const result = await getSubmissionsPaged(1, 100, { assignmentId })
  return result.items
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
