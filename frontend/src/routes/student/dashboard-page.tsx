import { useAuthStore } from "@/store/auth-store"

export function StudentDashboardPage() {
  const name = useAuthStore((s) => s.name)

  return (
    <div>
      <h1 className="text-2xl font-semibold">Welcome, {name}</h1>
      <p className="text-muted-foreground">
        View assignments for your class, submit your answers, and check
        your marks and feedback once graded.
      </p>
    </div>
  )
}
