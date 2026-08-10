import { zodResolver } from "@hookform/resolvers/zod"
import { isAxiosError } from "axios"
import { useState } from "react"
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
import {
  createSubmission,
  getSubmissions,
  updateSubmission,
} from "@/features/submissions/api"
import { useAsyncList } from "@/hooks/use-async-list"
import type { Assignment } from "@/types/assignment"
import type { Submission } from "@/types/submission"

const submissionSchema = z.object({
  answerText: z.string().min(1, "Answer is required"),
})

type SubmissionFormValues = z.infer<typeof submissionSchema>

export function StudentAssignmentsPage() {
  const {
    data: assignments,
    isLoading,
    error,
  } = useAsyncList(getAssignments)
  const { data: submissions, refetch: refetchSubmissions } =
    useAsyncList(getSubmissions)
  const [now] = useState(() => Date.now())

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
              <TableHead>Class</TableHead>
              <TableHead>Subject</TableHead>
              <TableHead>Deadline</TableHead>
              <TableHead>Max Marks</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Actions</TableHead>
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
            {assignments.map((a) => {
              const submission = submissions.find((s) => s.assignmentId === a.id)
              const deadlinePassed = new Date(a.deadline).getTime() < now
              const canUpdate =
                submission &&
                submission.status !== "Graded" &&
                a.allowResubmission &&
                !deadlinePassed

              return (
                <TableRow key={a.id}>
                  <TableCell>{a.title}</TableCell>
                  <TableCell>{a.className}</TableCell>
                  <TableCell>{a.subjectName}</TableCell>
                  <TableCell>{new Date(a.deadline).toLocaleString()}</TableCell>
                  <TableCell>{a.maxMarks}</TableCell>
                  <TableCell>
                    {submission ? (
                      <Badge
                        variant={
                          submission.status === "Graded" ? "default" : "secondary"
                        }
                      >
                        {submission.status}
                      </Badge>
                    ) : (
                      <Badge variant="outline">Not submitted</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    {!submission && !deadlinePassed && (
                      <ResourceDialog
                        triggerLabel="Submit"
                        triggerVariant="outline"
                        triggerSize="sm"
                        title={a.title}
                      >
                        {(close) => (
                          <SubmissionForm
                            assignment={a}
                            onSaved={() => {
                              refetchSubmissions()
                              close()
                            }}
                          />
                        )}
                      </ResourceDialog>
                    )}
                    {canUpdate && (
                      <ResourceDialog
                        triggerLabel="Update"
                        triggerVariant="outline"
                        triggerSize="sm"
                        title={a.title}
                      >
                        {(close) => (
                          <SubmissionForm
                            assignment={a}
                            submission={submission}
                            onSaved={() => {
                              refetchSubmissions()
                              close()
                            }}
                          />
                        )}
                      </ResourceDialog>
                    )}
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function SubmissionForm({
  assignment,
  submission,
  onSaved,
}: {
  assignment: Assignment
  submission?: Submission
  onSaved: () => void
}) {
  const form = useForm<SubmissionFormValues>({
    resolver: zodResolver(submissionSchema),
    defaultValues: { answerText: submission?.answerText ?? "" },
  })

  async function onSubmit(values: SubmissionFormValues) {
    try {
      if (submission) {
        await updateSubmission(submission.id, values)
        toast.success("Submission updated.")
      } else {
        await createSubmission({ assignmentId: assignment.id, ...values })
        toast.success("Answer submitted.")
      }
      onSaved()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to submit answer.")
        : "Failed to submit answer."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        <p className="text-sm text-muted-foreground">{assignment.description}</p>
        <FormField
          control={form.control}
          name="answerText"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Your Answer</FormLabel>
              <FormControl>
                <Textarea rows={6} {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting
            ? "Saving..."
            : submission
              ? "Update Answer"
              : "Submit"}
        </Button>
      </form>
    </Form>
  )
}
