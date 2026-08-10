import { useAuthStore } from "@/store/auth-store"

export function TeacherDashboardPage() {
  const name = useAuthStore((s) => s.name)

  return (
    <div>
      <h1 className="text-2xl font-semibold">Welcome, {name}</h1>
      <p className="text-muted-foreground">
        Create and publish assignments, review student submissions, and
        assign marks and feedback.
      </p>
    </div>
  )
}
