import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { getAssignments } from "@/features/assignments/api"
import { useAsyncList } from "@/hooks/use-async-list"

export function AssignmentsPage() {
  const {
    data: assignments,
    isLoading,
    error,
  } = useAsyncList(getAssignments)

  return (
    <div className="grid gap-4">
      <h1 className="text-2xl font-semibold">Assignments</h1>

      {isLoading && <p className="text-muted-foreground">Loading...</p>}
      {error && <p className="text-destructive">{error}</p>}

      {!isLoading && !error && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Title</TableHead>
              <TableHead>Teacher</TableHead>
              <TableHead>Class</TableHead>
              <TableHead>Subject</TableHead>
              <TableHead>Deadline</TableHead>
              <TableHead>Max Marks</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {assignments.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} className="text-muted-foreground">
                  No assignments yet.
                </TableCell>
              </TableRow>
            )}
            {assignments.map((a) => (
              <TableRow key={a.id}>
                <TableCell>{a.title}</TableCell>
                <TableCell>{a.teacherName}</TableCell>
                <TableCell>{a.className}</TableCell>
                <TableCell>{a.subjectName}</TableCell>
                <TableCell>
                  {new Date(a.deadline).toLocaleString()}
                </TableCell>
                <TableCell>{a.maxMarks}</TableCell>
                <TableCell>
                  <Badge variant={a.status === "Published" ? "default" : "secondary"}>
                    {a.status}
                  </Badge>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}
