export interface ClassSubject {
  id: string
  classId: string
  className: string
  subjectId: string
  subjectName: string
}

export interface CreateClassSubjectRequest {
  classId: string
  subjectId: string
}
