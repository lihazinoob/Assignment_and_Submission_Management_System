import { isAxiosError } from "axios"
import { useMemo, useState } from "react"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Pagination } from "@/components/ui/pagination"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  activateUser,
  deactivateUser,
  getUsersPaged,
} from "@/features/users/api"
import { usePagedList } from "@/hooks/use-paged-list"
import type { UserRole } from "@/types/auth"
import type { User } from "@/types/user"

export function UsersPage() {
  return (
    <div className="grid gap-4">
      <div>
        <h1 className="text-2xl font-semibold">Users</h1>
        <p className="text-muted-foreground text-sm">
          Teachers and Students create their own accounts by registering.
          Admins can deactivate an account to block sign-in, or reactivate it
          later.
        </p>
      </div>

      <Tabs defaultValue="teachers">
        <TabsList>
          <TabsTrigger value="teachers">Teachers</TabsTrigger>
          <TabsTrigger value="students">Students</TabsTrigger>
        </TabsList>

        <TabsContent value="teachers">
          <RoleUserTable role="Teacher" emptyLabel="No teachers found." />
        </TabsContent>
        <TabsContent value="students">
          <RoleUserTable role="Student" emptyLabel="No students found." />
        </TabsContent>
      </Tabs>
    </div>
  )
}

const ACTIVE_OPTIONS = ["all", "active", "inactive"] as const
type ActiveOption = (typeof ACTIVE_OPTIONS)[number]
const ACTIVE_LABELS: Record<ActiveOption, string> = {
  all: "All",
  active: "Active",
  inactive: "Inactive",
}

function RoleUserTable({
  role,
  emptyLabel,
}: {
  role: UserRole
  emptyLabel: string
}) {
  const [search, setSearch] = useState("")
  const [activeFilter, setActiveFilter] = useState<ActiveOption>("all")

  const filters = useMemo(
    () => ({
      role,
      isActive: activeFilter === "all" ? undefined : activeFilter === "active",
      search: search.trim() || undefined,
    }),
    [role, activeFilter, search]
  )

  const fetcher = useMemo(
    () => (page: number, pageSize: number, f: typeof filters) =>
      getUsersPaged(page, pageSize, f),
    []
  )

  const {
    data: users,
    totalCount,
    totalPages,
    page,
    setPage,
    isLoading,
    error,
    refetch,
  } = usePagedList(fetcher, filters, 10)

  return (
    <div className="grid gap-4">
      <div className="flex flex-wrap items-end gap-3">
        <div className="grid gap-2">
          <label className="text-sm leading-none font-medium">Search</label>
          <Input
            className="w-56"
            placeholder="Search by name or email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div className="grid gap-2">
          <label className="text-sm leading-none font-medium">Status</label>
          <Select
            value={activeFilter}
            onValueChange={(value: string | null) =>
              setActiveFilter((value as ActiveOption) ?? "all")
            }
          >
            <SelectTrigger className="w-36">
              <SelectValue placeholder="All">
                {(value: string | null) =>
                  ACTIVE_LABELS[(value as ActiveOption) ?? "all"]
                }
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              {ACTIVE_OPTIONS.map((option) => (
                <SelectItem key={option} value={option}>
                  {ACTIVE_LABELS[option]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <UserTable
        data={users}
        totalCount={totalCount}
        totalPages={totalPages}
        page={page}
        setPage={setPage}
        isLoading={isLoading}
        error={error}
        refetch={refetch}
        emptyLabel={emptyLabel}
      />
    </div>
  )
}

function UserTable({
  data: users,
  totalCount,
  totalPages,
  page,
  setPage,
  isLoading,
  error,
  refetch,
  emptyLabel,
}: {
  data: User[]
  totalCount: number
  totalPages: number
  page: number
  setPage: (page: number) => void
  isLoading: boolean
  error: string | null
  refetch: () => void
  emptyLabel: string
}) {
  if (isLoading) {
    return <p className="text-muted-foreground">Loading...</p>
  }
  if (error) {
    return <p className="text-destructive">{error}</p>
  }

  async function handleToggleActive(user: User) {
    const action = user.isActive ? "deactivate" : "activate"
    if (
      !confirm(
        user.isActive
          ? `Deactivate ${user.fullName}? They won't be able to sign in until reactivated.`
          : `Reactivate ${user.fullName}? They'll be able to sign in again.`
      )
    ) {
      return
    }

    try {
      if (user.isActive) {
        await deactivateUser(user.id)
      } else {
        await activateUser(user.id)
      }
      toast.success(`${user.fullName} ${action}d.`)
      refetch()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? `Failed to ${action} user.`)
        : `Failed to ${action} user.`
      toast.error(message)
    }
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.length === 0 && (
            <TableRow>
              <TableCell colSpan={4} className="text-muted-foreground">
                {emptyLabel}
              </TableCell>
            </TableRow>
          )}
          {users.map((u) => (
            <TableRow key={u.id}>
              <TableCell>{u.fullName}</TableCell>
              <TableCell>{u.email}</TableCell>
              <TableCell>
                <Badge variant={u.isActive ? "default" : "secondary"}>
                  {u.isActive ? "Active" : "Inactive"}
                </Badge>
              </TableCell>
              <TableCell>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleToggleActive(u)}
                >
                  {u.isActive ? "Deactivate" : "Activate"}
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <Pagination
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        onPageChange={setPage}
      />
    </>
  )
}
