import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type {
  CreateTeacherSubjectAssignmentRequest,
  TeacherSubjectAssignment,
} from "@/types/teacher-subject-assignment"

export async function getTeacherSubjectAssignmentsPaged(page: number, pageSize: number) {
  const { data } = await apiClient.get<PagedResponse<TeacherSubjectAssignment>>(
    "/teacher-subject-assignments",
    { params: { page, pageSize } }
  )
  return data
}

export async function getTeacherSubjectAssignments() {
  const result = await getTeacherSubjectAssignmentsPaged(1, 100)
  return result.items
}

export async function createTeacherSubjectAssignment(
  request: CreateTeacherSubjectAssignmentRequest
) {
  const { data } = await apiClient.post<TeacherSubjectAssignment>(
    "/teacher-subject-assignments",
    request
  )
  return data
}
