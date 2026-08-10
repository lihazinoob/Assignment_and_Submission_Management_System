import { apiClient } from "@/api/client"
import type { CreateUserRequest, User } from "@/types/user"

export async function createUser(request: CreateUserRequest) {
  const { data } = await apiClient.post<User>("/users", request)
  return data
}
