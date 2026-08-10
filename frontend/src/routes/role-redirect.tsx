import { Navigate } from "react-router-dom"

import { useAuthStore } from "@/store/auth-store"
import type { UserRole } from "@/types/auth"

const roleHome: Record<UserRole, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
}

export function RoleRedirect() {
  const role = useAuthStore((s) => s.role)

  if (!role) {
    return <Navigate to="/login" replace />
  }

  return <Navigate to={roleHome[role]} replace />
}
