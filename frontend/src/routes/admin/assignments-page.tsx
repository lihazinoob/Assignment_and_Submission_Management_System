import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { ResourceDialog } from "@/components/resource-dialog"
import { getAssignments } from "@/features/assignments/api"
import { useAsyncList } from "@/hooks/use-async-list"
import type { Assignment } from "@/types/assignment"

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
              <TableHead>Details</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {assignments.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-muted-foreground">
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
                <TableCell>
                  <ResourceDialog
                    triggerLabel="View"
                    triggerVariant="outline"
                    triggerSize="sm"
                    title={a.title}
                  >
                    {() => <AssignmentDetails assignment={a} />}
                  </ResourceDialog>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function AssignmentDetails({ assignment }: { assignment: Assignment }) {
  return (
    <dl className="grid gap-3 text-sm">
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Teacher</dt>
        <dd className="col-span-2">{assignment.teacherName}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Class</dt>
        <dd className="col-span-2">{assignment.className}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Subject</dt>
        <dd className="col-span-2">{assignment.subjectName}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Description</dt>
        <dd className="col-span-2 whitespace-pre-wrap">
          {assignment.description || "—"}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Deadline</dt>
        <dd className="col-span-2">
          {new Date(assignment.deadline).toLocaleString()}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Max Marks</dt>
        <dd className="col-span-2">{assignment.maxMarks}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Allow Resubmission</dt>
        <dd className="col-span-2">
          {assignment.allowResubmission ? "Yes" : "No"}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Status</dt>
        <dd className="col-span-2">
          <Badge variant={assignment.status === "Published" ? "default" : "secondary"}>
            {assignment.status}
          </Badge>
        </dd>
      </div>
    </dl>
  )
}
