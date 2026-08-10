import {
  BookOpen,
  ClipboardList,
  GraduationCap,
  LayoutDashboard,
  Link2,
  School,
  UserCog,
  Users,
} from "lucide-react"

import { AppShell } from "@/components/layout/app-shell"
import type { NavItem } from "@/components/layout/sidebar"

const navItems: NavItem[] = [
  { label: "Dashboard", to: "/admin", icon: LayoutDashboard, end: true },
  { label: "Users", to: "/admin/users", icon: Users },
  { label: "Classes", to: "/admin/classes", icon: School },
  { label: "Subjects", to: "/admin/subjects", icon: BookOpen },
  { label: "Class-Subjects", to: "/admin/class-subjects", icon: Link2 },
  {
    label: "Teacher Assignments",
    to: "/admin/teacher-assignments",
    icon: UserCog,
  },
  { label: "Assignments", to: "/admin/assignments", icon: ClipboardList },
  { label: "Enrollments", to: "/admin/enrollments", icon: GraduationCap },
]

export function AdminLayout() {
  return <AppShell navItems={navItems} roleLabel="Admin" />
}
