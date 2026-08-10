import { ClipboardList, LayoutDashboard, NotebookPen } from "lucide-react"

import { AppShell } from "@/components/layout/app-shell"
import type { NavItem } from "@/components/layout/sidebar"

const navItems: NavItem[] = [
  { label: "Dashboard", to: "/student", icon: LayoutDashboard, end: true },
  { label: "Assignments", to: "/student/assignments", icon: ClipboardList },
  { label: "My Submissions", to: "/student/submissions", icon: NotebookPen },
]

export function StudentLayout() {
  return <AppShell navItems={navItems} roleLabel="Student" />
}
