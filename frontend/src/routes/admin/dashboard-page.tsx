import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { getAssignments } from "@/features/assignments/api"
import { getClasses } from "@/features/classes/api"
import { getStudentEnrollments } from "@/features/student-enrollments/api"
import { getSubmissions } from "@/features/submissions/api"
import { getSubjects } from "@/features/subjects/api"
import { getTeacherSubjectAssignments } from "@/features/teacher-subject-assignments/api"
import { getStudents, getTeachers } from "@/features/users/api"
import { useAsyncList } from "@/hooks/use-async-list"
import { useAuthStore } from "@/store/auth-store"

const CHART_COLORS = [
  "var(--chart-2)",
  "var(--chart-4)",
  "var(--chart-3)",
  "var(--chart-5)",
  "var(--chart-1)",
]

export function AdminDashboardPage() {
  const name = useAuthStore((s) => s.name)

  const teachers = useAsyncList(getTeachers)
  const students = useAsyncList(getStudents)
  const classes = useAsyncList(getClasses)
  const subjects = useAsyncList(getSubjects)
  const assignments = useAsyncList(getAssignments)
  const submissions = useAsyncList(getSubmissions)
  const enrollments = useAsyncList(getStudentEnrollments)
  const teacherSubjectAssignments = useAsyncList(getTeacherSubjectAssignments)

  const isLoading =
    teachers.isLoading ||
    students.isLoading ||
    classes.isLoading ||
    subjects.isLoading ||
    assignments.isLoading ||
    submissions.isLoading ||
    enrollments.isLoading ||
    teacherSubjectAssignments.isLoading

  const usersByRole = [
    { name: "Teachers", value: teachers.data.length },
    { name: "Students", value: students.data.length },
  ].filter((d) => d.value > 0)

  const assignmentsByStatus = ["Draft", "Published"].map((status) => ({
    name: status,
    count: assignments.data.filter((a) => a.status === status).length,
  }))

  const submissionsByStatus = ["Submitted", "Late", "Graded"].map((status) => ({
    name: status,
    count: submissions.data.filter((s) => s.status === status).length,
  }))

  const enrollmentsByClass = Object.entries(
    enrollments.data.reduce<Record<string, number>>((acc, e) => {
      acc[e.className] = (acc[e.className] ?? 0) + 1
      return acc
    }, {})
  ).map(([name, count]) => ({ name, count }))

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Welcome, {name}</h1>
        <p className="text-muted-foreground">
          An overview of everyone and everything currently in the system.
        </p>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading dashboard...</p>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6">
            <StatCard label="Teachers" value={teachers.data.length} />
            <StatCard label="Students" value={students.data.length} />
            <StatCard label="Classes" value={classes.data.length} />
            <StatCard label="Subjects" value={subjects.data.length} />
            <StatCard label="Assignments" value={assignments.data.length} />
            <StatCard label="Submissions" value={submissions.data.length} />
          </div>

          <div className="grid gap-4 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Teachers vs Students</CardTitle>
              </CardHeader>
              <CardContent>
                {usersByRole.length === 0 ? (
                  <EmptyChartState />
                ) : (
                  <ResponsiveContainer width="100%" height={260}>
                    <PieChart>
                      <Pie
                        data={usersByRole}
                        dataKey="value"
                        nameKey="name"
                        innerRadius={55}
                        outerRadius={90}
                        paddingAngle={2}
                        label={({ name, value }) => `${name}: ${value}`}
                      >
                        {usersByRole.map((entry, i) => (
                          <Cell key={entry.name} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                        ))}
                      </Pie>
                      <Legend />
                      <Tooltip
                        contentStyle={{
                          background: "var(--popover)",
                          border: "1px solid var(--border)",
                          borderRadius: "var(--radius-md)",
                          color: "var(--popover-foreground)",
                        }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Assignments by Status</CardTitle>
              </CardHeader>
              <CardContent>
                {assignments.data.length === 0 ? (
                  <EmptyChartState />
                ) : (
                  <ResponsiveContainer width="100%" height={260}>
                    <BarChart data={assignmentsByStatus}>
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis
                        dataKey="name"
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <YAxis
                        allowDecimals={false}
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <Tooltip
                        cursor={{ fill: "var(--muted)" }}
                        contentStyle={{
                          background: "var(--popover)",
                          border: "1px solid var(--border)",
                          borderRadius: "var(--radius-md)",
                          color: "var(--popover-foreground)",
                        }}
                      />
                      <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                        {assignmentsByStatus.map((entry, i) => (
                          <Cell key={entry.name} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Submissions by Status</CardTitle>
              </CardHeader>
              <CardContent>
                {submissions.data.length === 0 ? (
                  <EmptyChartState />
                ) : (
                  <ResponsiveContainer width="100%" height={260}>
                    <BarChart data={submissionsByStatus}>
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis
                        dataKey="name"
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <YAxis
                        allowDecimals={false}
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <Tooltip
                        cursor={{ fill: "var(--muted)" }}
                        contentStyle={{
                          background: "var(--popover)",
                          border: "1px solid var(--border)",
                          borderRadius: "var(--radius-md)",
                          color: "var(--popover-foreground)",
                        }}
                      />
                      <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                        {submissionsByStatus.map((entry, i) => (
                          <Cell key={entry.name} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Enrollments by Class</CardTitle>
              </CardHeader>
              <CardContent>
                {enrollmentsByClass.length === 0 ? (
                  <EmptyChartState />
                ) : (
                  <ResponsiveContainer width="100%" height={260}>
                    <BarChart data={enrollmentsByClass}>
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis
                        dataKey="name"
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <YAxis
                        allowDecimals={false}
                        stroke="var(--muted-foreground)"
                        fontSize={12}
                      />
                      <Tooltip
                        cursor={{ fill: "var(--muted)" }}
                        contentStyle={{
                          background: "var(--popover)",
                          border: "1px solid var(--border)",
                          borderRadius: "var(--radius-md)",
                          color: "var(--popover-foreground)",
                        }}
                      />
                      <Bar
                        dataKey="count"
                        fill="var(--chart-2)"
                        radius={[4, 4, 0, 0]}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                )}
              </CardContent>
            </Card>
          </div>

          <p className="text-muted-foreground text-sm">
            {teacherSubjectAssignments.data.length} teacher-subject assignment
            {teacherSubjectAssignments.data.length === 1 ? "" : "s"} configured
            across {classes.data.length} class
            {classes.data.length === 1 ? "" : "es"} and {subjects.data.length}{" "}
            subject{subjects.data.length === 1 ? "" : "s"}.
          </p>
        </>
      )}
    </div>
  )
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <Card size="sm">
      <CardContent>
        <p className="text-muted-foreground text-xs">{label}</p>
        <p className="font-heading text-2xl font-semibold">{value}</p>
      </CardContent>
    </Card>
  )
}

function EmptyChartState() {
  return (
    <div className="text-muted-foreground flex h-[260px] items-center justify-center text-sm">
      No data yet.
    </div>
  )
}
