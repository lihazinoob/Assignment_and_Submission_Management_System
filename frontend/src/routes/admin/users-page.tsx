import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { getStudents, getTeachers } from "@/features/users/api"
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
          Teachers and Students create their own accounts by registering. This
          is a read-only view of everyone currently in the system.
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
  emptyLabel,
}: {
  data: User[]
  isLoading: boolean
  error: string | null
  emptyLabel: string
}) {
  if (isLoading) {
    return <p className="text-muted-foreground">Loading...</p>
  }
  if (error) {
    return <p className="text-destructive">{error}</p>
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>Email</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {users.length === 0 && (
          <TableRow>
            <TableCell colSpan={3} className="text-muted-foreground">
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
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
