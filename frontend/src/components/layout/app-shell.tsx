import { Outlet, useNavigate } from "react-router-dom"
import { LogOut } from "lucide-react"

import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { type NavItem, Sidebar } from "@/components/layout/sidebar"
import { useAuthStore } from "@/store/auth-store"

interface AppShellProps {
  navItems: NavItem[]
  roleLabel: string
}

export function AppShell({ navItems, roleLabel }: AppShellProps) {
  const navigate = useNavigate()
  const name = useAuthStore((s) => s.name)
  const clearSession = useAuthStore((s) => s.clearSession)

  function handleLogout() {
    clearSession()
    navigate("/login", { replace: true })
  }

  const initials = name
    ?.split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase()

  return (
    <div className="flex min-h-svh">
      <Sidebar navItems={navItems} roleLabel={roleLabel} />
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b px-6 py-3">
          <div className="flex items-center gap-2">
            <Avatar className="size-8">
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
            <span className="text-sm font-medium">{name}</span>
          </div>
          <Button variant="ghost" size="sm" onClick={handleLogout}>
            <LogOut className="size-4" />
            Log out
          </Button>
        </header>
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
