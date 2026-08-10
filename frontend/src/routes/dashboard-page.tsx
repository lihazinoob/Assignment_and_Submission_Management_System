import { useAuthStore } from "@/store/auth-store"

export function DashboardPage() {
  const { name, role } = useAuthStore()

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold">Welcome, {name}</h1>
      <p className="text-muted-foreground">Role: {role}</p>
    </div>
  )
}
