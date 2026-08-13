import { apiClient } from "@/api/client"
import type { PagedResponse } from "@/types/api"
import type {
  CreateStudentEnrollmentRequest,
  StudentEnrollment,
} from "@/types/student-enrollment"

export async function getStudentEnrollmentsPaged(page: number, pageSize: number) {
  const { data } = await apiClient.get<PagedResponse<StudentEnrollment>>(
    "/student-enrollments",
    { params: { page, pageSize } }
  )
  return data
}

export async function getStudentEnrollments() {
  const result = await getStudentEnrollmentsPaged(1, 100)
  return result.items
}

export async function createStudentEnrollment(
  request: CreateStudentEnrollmentRequest
) {
  const { data } = await apiClient.post<StudentEnrollment>(
    "/student-enrollments",
    request
  )
  return data
}
