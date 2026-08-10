export interface SchoolClass {
  id: string
  name: string
  academicYear: string
  isActive: boolean
}

export interface CreateClassRequest {
  name: string
  academicYear: string
}
