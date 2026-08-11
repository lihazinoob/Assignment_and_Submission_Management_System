import type { UserRole } from "@/types/auth"

export interface User {
  id: string
  fullName: string
  email: string
  role: UserRole
  isActive: boolean
}
