import { ClipboardList, LayoutDashboard, NotebookPen } from "lucide-react"

import { AppShell } from "@/components/layout/app-shell"
import type { NavItem } from "@/components/layout/sidebar"

const navItems: NavItem[] = [
  { label: "Dashboard", to: "/teacher", icon: LayoutDashboard, end: true },
  { label: "My Assignments", to: "/teacher/assignments", icon: ClipboardList },
  { label: "Submissions", to: "/teacher/submissions", icon: NotebookPen },
]

export function TeacherLayout() {
  return <AppShell navItems={navItems} roleLabel="Teacher" />
}
