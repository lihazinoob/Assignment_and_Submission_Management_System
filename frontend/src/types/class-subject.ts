export interface ClassSubject {
  id: string
  classId: string
  className: string
  subjectId: string
  subjectName: string
  isActive: boolean
}

export interface CreateClassSubjectRequest {
  classId: string
  subjectId: string
}
