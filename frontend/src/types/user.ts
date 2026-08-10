import type { UserRole } from "@/types/auth"

export interface User {
  id: string
  fullName: string
  email: string
  role: UserRole
  isActive: boolean
}

export interface CreateUserRequest {
  fullName: string
  email: string
  password: string
  role: UserRole
}
