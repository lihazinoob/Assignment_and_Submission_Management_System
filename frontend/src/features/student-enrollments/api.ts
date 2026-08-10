import { apiClient } from "@/api/client"
import type {
  CreateStudentEnrollmentRequest,
  StudentEnrollment,
} from "@/types/student-enrollment"

export async function getStudentEnrollments() {
  const { data } = await apiClient.get<StudentEnrollment[]>(
    "/student-enrollments"
  )
  return data
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
