import { apiClient } from "@/api/client"
import type {
  CreateTeacherSubjectAssignmentRequest,
  TeacherSubjectAssignment,
} from "@/types/teacher-subject-assignment"

export async function getTeacherSubjectAssignments() {
  const { data } = await apiClient.get<TeacherSubjectAssignment[]>(
    "/teacher-subject-assignments"
  )
  return data
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
