export type AssignmentStatus = "Draft" | "Published"

export interface Assignment {
  id: string
  teacherSubjectAssignmentId: string
  teacherName: string
  className: string
  subjectName: string
  title: string
  description: string
  deadline: string
  maxMarks: number
  allowResubmission: boolean
  status: AssignmentStatus
}

export interface CreateAssignmentRequest {
  teacherSubjectAssignmentId: string
  title: string
  description: string
  deadline: string
  maxMarks: number
  allowResubmission: boolean
}

export interface UpdateAssignmentRequest {
  title: string
  description: string
  deadline: string
  maxMarks: number
  allowResubmission: boolean
}
