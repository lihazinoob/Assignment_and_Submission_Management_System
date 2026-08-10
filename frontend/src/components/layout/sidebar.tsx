import { NavLink } from "react-router-dom"
import type { LucideIcon } from "lucide-react"

import { cn } from "@/lib/utils"

export interface NavItem {
  label: string
  to: string
  icon: LucideIcon
  end?: boolean
}

interface SidebarProps {
  navItems: NavItem[]
  roleLabel: string
}

export function Sidebar({ navItems, roleLabel }: SidebarProps) {
  return (
    <aside className="hidden w-56 shrink-0 border-r bg-sidebar text-sidebar-foreground md:flex md:flex-col">
      <div className="px-4 py-4 text-lg font-semibold">LMS · {roleLabel}</div>
      <nav className="flex flex-col gap-1 px-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
              )
            }
          >
            <item.icon className="size-4" />
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
