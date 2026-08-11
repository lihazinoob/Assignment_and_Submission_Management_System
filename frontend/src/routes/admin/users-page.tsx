import { isAxiosError } from "axios"
import { toast } from "sonner"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
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
  getStudents,
  getTeachers,
} from "@/features/users/api"
import { useAsyncList } from "@/hooks/use-async-list"
import type { User } from "@/types/user"

export function UsersPage() {
  const teachers = useAsyncList(getTeachers)
  const students = useAsyncList(getStudents)

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
          <TabsTrigger value="teachers">
            Teachers {!teachers.isLoading && `(${teachers.data.length})`}
          </TabsTrigger>
          <TabsTrigger value="students">
            Students {!students.isLoading && `(${students.data.length})`}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="teachers">
          <UserTable {...teachers} emptyLabel="No teachers yet." />
        </TabsContent>
        <TabsContent value="students">
          <UserTable {...students} emptyLabel="No students yet." />
        </TabsContent>
      </Tabs>
    </div>
  )
}

function UserTable({
  data: users,
  isLoading,
  error,
  refetch,
  emptyLabel,
}: {
  data: User[]
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
  )
}
