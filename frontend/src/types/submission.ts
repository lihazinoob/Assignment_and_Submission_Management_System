export type SubmissionStatus = "Submitted" | "Late" | "Graded"

export interface Submission {
  id: string
  assignmentId: string
  assignmentTitle: string
  studentId: string
  studentName: string
  answerText: string
  submittedAt: string
  status: SubmissionStatus
  marksObtained: number | null
  feedback: string | null
  gradedBy: string | null
  gradedAt: string | null
}

export interface CreateSubmissionRequest {
  assignmentId: string
  answerText: string
}

export interface UpdateSubmissionRequest {
  answerText: string
}

export interface GradeSubmissionRequest {
  marksObtained: number
  feedback: string
}
