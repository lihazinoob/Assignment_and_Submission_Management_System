import { apiClient } from "@/api/client"
import type { AuthResponse, LoginRequest } from "@/types/auth"

export async function login(request: LoginRequest) {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", request)
  return data
}
