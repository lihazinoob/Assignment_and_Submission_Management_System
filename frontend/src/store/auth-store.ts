import { create } from "zustand"
import { persist } from "zustand/middleware"

import { decodeJwt } from "@/lib/jwt"
import type { AuthResponse, UserRole } from "@/types/auth"

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  userId: string | null
  email: string | null
  role: UserRole | null
  name: string | null
  setSession: (auth: AuthResponse) => void
  clearSession: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      userId: null,
      email: null,
      role: null,
      name: null,
      setSession: (auth) => {
        const claims = decodeJwt(auth.accessToken)
        set({
          accessToken: auth.accessToken,
          refreshToken: auth.refreshToken,
          userId: claims.sub,
          email: claims.email,
          role: claims.role,
          name: claims.name,
        })
      },
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          userId: null,
          email: null,
          role: null,
          name: null,
        }),
    }),
    { name: "lms-auth" }
  )
)
