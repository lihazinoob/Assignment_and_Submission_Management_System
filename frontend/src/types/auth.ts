export type UserRole = "Admin" | "Teacher" | "Student"

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
  role: "Teacher" | "Student"
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
}

export interface JwtClaims {
  sub: string
  email: string
  role: UserRole
  name: string
  exp: number
}
