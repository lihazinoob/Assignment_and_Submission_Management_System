import { zodResolver } from "@hookform/resolvers/zod"
import { isAxiosError } from "axios"
import { useMemo, useState } from "react"
import { useForm } from "react-hook-form"
import { toast } from "sonner"
import { z } from "zod"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { Pagination } from "@/components/ui/pagination"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Textarea } from "@/components/ui/textarea"
import { ResourceDialog } from "@/components/resource-dialog"
import { getAssignments } from "@/features/assignments/api"
import { getSubmissionsPaged, gradeSubmission } from "@/features/submissions/api"
import type { SubmissionFilters } from "@/features/submissions/api"
import { useAsyncList } from "@/hooks/use-async-list"
import { usePagedList } from "@/hooks/use-paged-list"
import type { SubmissionStatus } from "@/types/submission"
import type { Submission } from "@/types/submission"

const STATUS_OPTIONS = ["all", "Submitted", "Late", "Graded"] as const
type StatusOption = (typeof STATUS_OPTIONS)[number]

export function TeacherSubmissionsPage() {
  const { data: assignments } = useAsyncList(getAssignments)
  const [assignmentId, setAssignmentId] = useState("")
  const [status, setStatus] = useState<StatusOption>("all")

  const filters = useMemo<SubmissionFilters>(
    () => ({
      assignmentId: assignmentId || undefined,
      status: status === "all" ? undefined : (status as SubmissionStatus),
    }),
    [assignmentId, status]
  )

  const fetcher = useMemo(
    () => (page: number, pageSize: number, f: SubmissionFilters) =>
      assignmentId
        ? getSubmissionsPaged(page, pageSize, f)
        : Promise.resolve({ items: [], totalCount: 0, page: 1, pageSize }),
    [assignmentId]
  )

  const {
    data: submissions,
    totalCount,
    totalPages,
    page,
    setPage,
    isLoading,
    error,
    refetch,
  } = usePagedList(fetcher, filters, 10)

  const selectedAssignment = assignments.find((a) => a.id === assignmentId)

  return (
    <div className="grid gap-4">
      <h1 className="text-2xl font-semibold">Submissions</h1>

      <div className="flex flex-wrap items-end gap-3">
        <div className="grid max-w-sm min-w-64 gap-2">
          <label className="text-sm leading-none font-medium">
            Assignment
          </label>
          <Select
            value={assignmentId}
            onValueChange={(value: string | null) => setAssignmentId(value ?? "")}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Select an assignment">
                {(value: string | null) => {
                  if (!value) return "Select an assignment"
                  const selected = assignments.find((a) => a.id === value)
                  return selected ? selected.title : value
                }}
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              {assignments.map((a) => (
                <SelectItem key={a.id} value={a.id}>
                  {a.title} ({a.className} · {a.subjectName})
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {assignmentId && (
          <div className="grid gap-2">
            <label className="text-sm leading-none font-medium">Status</label>
            <Select
              value={status}
              onValueChange={(value: string | null) =>
                setStatus((value as StatusOption) ?? "all")
              }
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="All statuses">
                  {(value: string | null) =>
                    value === "all" || !value ? "All statuses" : value
                  }
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {STATUS_OPTIONS.map((option) => (
                  <SelectItem key={option} value={option}>
                    {option === "all" ? "All statuses" : option}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        )}
      </div>

      {!assignmentId && (
        <p className="text-muted-foreground">
          Select an assignment above to view its submissions.
        </p>
      )}

      {assignmentId && isLoading && (
        <p className="text-muted-foreground">Loading...</p>
      )}
      {assignmentId && error && <p className="text-destructive">{error}</p>}

      {assignmentId && !isLoading && !error && (
        <>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Student</TableHead>
              <TableHead>Answer</TableHead>
              <TableHead>Submitted</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Marks</TableHead>
              <TableHead>Feedback</TableHead>
              <TableHead>Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {submissions.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} className="text-muted-foreground">
                  No submissions yet.
                </TableCell>
              </TableRow>
            )}
            {submissions.map((s) => (
              <TableRow key={s.id}>
                <TableCell>{s.studentName}</TableCell>
                <TableCell className="max-w-xs truncate">
                  {s.answerText}
                </TableCell>
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
                <TableCell>
                  <ResourceDialog
                    triggerLabel={s.status === "Graded" ? "Re-grade" : "Grade"}
                    triggerVariant="outline"
                    triggerSize="sm"
                    title="Grade Submission"
                  >
                    {(close) => (
                      <GradeForm
                        submission={s}
                        maxMarks={selectedAssignment?.maxMarks ?? 100}
                        onGraded={() => {
                          refetch()
                          close()
                        }}
                      />
                    )}
                  </ResourceDialog>
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
      )}
    </div>
  )
}

function GradeForm({
  submission,
  maxMarks,
  onGraded,
}: {
  submission: Submission
  maxMarks: number
  onGraded: () => void
}) {
  const gradeSchema = z.object({
    marksObtained: z.coerce
      .number()
      .min(0, "Marks cannot be negative")
      .max(maxMarks, `Marks cannot exceed ${maxMarks}`),
    feedback: z.string().min(1, "Feedback is required"),
  })

  type GradeFormValues = z.infer<typeof gradeSchema>

  const form = useForm<z.input<typeof gradeSchema>, unknown, GradeFormValues>({
    resolver: zodResolver(gradeSchema),
    defaultValues: {
      marksObtained: submission.marksObtained ?? 0,
      feedback: submission.feedback ?? "",
    },
  })

  async function onSubmit(values: GradeFormValues) {
    try {
      await gradeSubmission(submission.id, values)
      toast.success("Submission graded.")
      onGraded()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to grade submission.")
        : "Failed to grade submission."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        <p className="text-sm text-muted-foreground">
          Answer: {submission.answerText}
        </p>
        <FormField
          control={form.control}
          name="marksObtained"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Marks (out of {maxMarks})</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min={0}
                  max={maxMarks}
                  {...field}
                  value={field.value as number | string}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="feedback"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Feedback</FormLabel>
              <FormControl>
                <Textarea placeholder="Good effort, minor errors." {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting ? "Saving..." : "Submit Grade"}
        </Button>
      </form>
    </Form>
  )
}
