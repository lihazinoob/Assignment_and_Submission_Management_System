import { useAuthStore } from "@/store/auth-store"

export function AdminDashboardPage() {
  const name = useAuthStore((s) => s.name)

  return (
    <div>
      <h1 className="text-2xl font-semibold">Welcome, {name}</h1>
      <p className="text-muted-foreground">
        Manage users, classes, subjects, and teacher assignments, and view
        all assignments and submissions across the system.
      </p>
    </div>
  )
}
