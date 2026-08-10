export interface Subject {
  id: string
  name: string
  code: string
  isActive: boolean
}

export interface CreateSubjectRequest {
  name: string
  code: string
}
