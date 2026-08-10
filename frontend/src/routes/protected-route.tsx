import { Navigate, Outlet } from "react-router-dom"

import { useAuthStore } from "@/store/auth-store"
import type { UserRole } from "@/types/auth"

interface ProtectedRouteProps {
  allowedRoles?: UserRole[]
}

export function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const { accessToken, role } = useAuthStore()

  if (!accessToken) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && role && !allowedRoles.includes(role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
