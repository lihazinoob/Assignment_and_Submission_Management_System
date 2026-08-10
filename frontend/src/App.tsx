import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

import { LoginPage } from "@/features/auth/login-page"
import { AdminLayout } from "@/layouts/admin-layout"
import { StudentLayout } from "@/layouts/student-layout"
import { TeacherLayout } from "@/layouts/teacher-layout"
import { AdminDashboardPage } from "@/routes/admin/dashboard-page"
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
              <Route path="users" element={<PlaceholderPage title="Users" />} />
              <Route
                path="classes"
                element={<PlaceholderPage title="Classes" />}
              />
              <Route
                path="subjects"
                element={<PlaceholderPage title="Subjects" />}
              />
              <Route
                path="class-subjects"
                element={<PlaceholderPage title="Class-Subjects" />}
              />
              <Route
                path="teacher-assignments"
                element={<PlaceholderPage title="Teacher Assignments" />}
              />
              <Route
                path="assignments"
                element={<PlaceholderPage title="Assignments" />}
              />
              <Route
                path="enrollments"
                element={<PlaceholderPage title="Enrollments" />}
              />
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
