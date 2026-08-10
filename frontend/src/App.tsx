import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

import { LoginPage } from "@/features/auth/login-page"
import { AdminLayout } from "@/layouts/admin-layout"
import { StudentLayout } from "@/layouts/student-layout"
import { TeacherLayout } from "@/layouts/teacher-layout"
import { AdminDashboardPage } from "@/routes/admin/dashboard-page"
import { ClassesPage } from "@/routes/admin/classes-page"
import { ClassSubjectsPage } from "@/routes/admin/class-subjects-page"
import { EnrollmentsPage } from "@/routes/admin/enrollments-page"
import { SubjectsPage } from "@/routes/admin/subjects-page"
import { TeacherAssignmentsPage } from "@/routes/admin/teacher-assignments-page"
import { UsersPage } from "@/routes/admin/users-page"
import { PlaceholderPage } from "@/routes/placeholder-page"
import { ProtectedRoute } from "@/routes/protected-route"
import { RoleRedirect } from "@/routes/role-redirect"
import { StudentDashboardPage } from "@/routes/student/dashboard-page"
import { TeacherDashboardPage } from "@/routes/teacher/dashboard-page"

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute />}>
          <Route index element={<RoleRedirect />} />

          <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
            <Route path="admin" element={<AdminLayout />}>
              <Route index element={<AdminDashboardPage />} />
              <Route path="users" element={<UsersPage />} />
              <Route path="classes" element={<ClassesPage />} />
              <Route path="subjects" element={<SubjectsPage />} />
              <Route path="class-subjects" element={<ClassSubjectsPage />} />
              <Route
                path="teacher-assignments"
                element={<TeacherAssignmentsPage />}
              />
              <Route
                path="assignments"
                element={<PlaceholderPage title="Assignments" />}
              />
              <Route path="enrollments" element={<EnrollmentsPage />} />
            </Route>
          </Route>

          <Route element={<ProtectedRoute allowedRoles={["Teacher"]} />}>
            <Route path="teacher" element={<TeacherLayout />}>
              <Route index element={<TeacherDashboardPage />} />
              <Route
                path="assignments"
                element={<PlaceholderPage title="My Assignments" />}
              />
              <Route
                path="submissions"
                element={<PlaceholderPage title="Submissions" />}
              />
            </Route>
          </Route>

          <Route element={<ProtectedRoute allowedRoles={["Student"]} />}>
            <Route path="student" element={<StudentLayout />}>
              <Route index element={<StudentDashboardPage />} />
              <Route
                path="assignments"
                element={<PlaceholderPage title="Assignments" />}
              />
              <Route
                path="submissions"
                element={<PlaceholderPage title="My Submissions" />}
              />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
