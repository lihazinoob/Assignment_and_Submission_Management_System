import { Badge } from "@/components/ui/badge"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { getSubmissions } from "@/features/submissions/api"
import { useAsyncList } from "@/hooks/use-async-list"

export function StudentSubmissionsPage() {
  const {
    data: submissions,
    isLoading,
    error,
  } = useAsyncList(getSubmissions)

  return (
    <div className="grid gap-4">
      <h1 className="text-2xl font-semibold">My Submissions</h1>

      {isLoading && <p className="text-muted-foreground">Loading...</p>}
      {error && <p className="text-destructive">{error}</p>}

      {!isLoading && !error && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Assignment</TableHead>
              <TableHead>Submitted</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Marks</TableHead>
              <TableHead>Feedback</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {submissions.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="text-muted-foreground">
                  No submissions yet.
                </TableCell>
              </TableRow>
            )}
            {submissions.map((s) => (
              <TableRow key={s.id}>
                <TableCell>{s.assignmentTitle}</TableCell>
                <TableCell>{new Date(s.submittedAt).toLocaleString()}</TableCell>
                <TableCell>
                  <Badge variant={s.status === "Graded" ? "default" : "secondary"}>
                    {s.status}
                  </Badge>
                </TableCell>
                <TableCell>{s.marksObtained ?? "—"}</TableCell>
                <TableCell className="max-w-xs truncate">
                  {s.feedback ?? "—"}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}
