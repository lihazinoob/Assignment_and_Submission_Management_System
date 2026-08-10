export interface StudentEnrollment {
  id: string
  studentId: string
  studentName: string
  classId: string
  className: string
  rollNumber: string | null
  status: string
  enrolledAt: string
}

export interface CreateStudentEnrollmentRequest {
  studentId: string
  classId: string
  rollNumber?: string
}
