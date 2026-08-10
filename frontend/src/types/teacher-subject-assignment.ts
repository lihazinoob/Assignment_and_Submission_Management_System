export interface TeacherSubjectAssignment {
  id: string
  teacherId: string
  teacherName: string
  classSubjectId: string
  className: string
  subjectName: string
  assignedBy: string
  assignedAt: string
}

export interface CreateTeacherSubjectAssignmentRequest {
  teacherId: string
  classSubjectId: string
}
