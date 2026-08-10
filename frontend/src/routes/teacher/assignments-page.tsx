import { zodResolver } from "@hookform/resolvers/zod"
import { isAxiosError } from "axios"
import { useState } from "react"
import { useForm } from "react-hook-form"
import { toast } from "sonner"
import { z } from "zod"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
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
import {
  createAssignment,
  deleteAssignment,
  getAssignments,
  publishAssignment,
  updateAssignment,
} from "@/features/assignments/api"
import { getTeacherSubjectAssignments } from "@/features/teacher-subject-assignments/api"
import { useAsyncList } from "@/hooks/use-async-list"
import { fromDatetimeLocalValue, toDatetimeLocalValue } from "@/lib/datetime"
import type { Assignment } from "@/types/assignment"

const assignmentSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: z.coerce.number().positive("Max marks must be greater than zero"),
  allowResubmission: z.boolean(),
})

type AssignmentFormValues = z.infer<typeof assignmentSchema>

export function MyAssignmentsPage() {
  const {
    data: assignments,
    isLoading,
    error,
    refetch,
  } = useAsyncList(getAssignments)

  async function handlePublish(id: string) {
    try {
      await publishAssignment(id)
      toast.success("Assignment published.")
      refetch()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to publish assignment.")
        : "Failed to publish assignment."
      toast.error(message)
    }
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this assignment? This cannot be undone.")) return
    try {
      await deleteAssignment(id)
      toast.success("Assignment deleted.")
      refetch()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to delete assignment.")
        : "Failed to delete assignment."
      toast.error(message)
    }
  }

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">My Assignments</h1>
        <ResourceDialog triggerLabel="Create Assignment" title="Create Assignment">
          {(close) => (
            <AssignmentForm
              onSaved={() => {
                refetch()
                close()
              }}
            />
          )}
        </ResourceDialog>
      </div>

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
            {assignments.map((a) => (
              <TableRow key={a.id}>
                <TableCell>{a.title}</TableCell>
                <TableCell>{a.className}</TableCell>
                <TableCell>{a.subjectName}</TableCell>
                <TableCell>{new Date(a.deadline).toLocaleString()}</TableCell>
                <TableCell>{a.maxMarks}</TableCell>
                <TableCell>
                  <Badge variant={a.status === "Published" ? "default" : "secondary"}>
                    {a.status}
                  </Badge>
                </TableCell>
                <TableCell>
                  {a.status === "Draft" && (
                    <div className="flex gap-2">
                      <ResourceDialog
                        triggerLabel="Edit"
                        triggerVariant="outline"
                        triggerSize="sm"
                        title="Edit Assignment"
                      >
                        {(close) => (
                          <AssignmentForm
                            assignment={a}
                            onSaved={() => {
                              refetch()
                              close()
                            }}
                          />
                        )}
                      </ResourceDialog>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handlePublish(a.id)}
                      >
                        Publish
                      </Button>
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={() => handleDelete(a.id)}
                      >
                        Delete
                      </Button>
                    </div>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function AssignmentForm({
  assignment,
  onSaved,
}: {
  assignment?: Assignment
  onSaved: () => void
}) {
  const { data: teacherSubjectAssignments } = useAsyncList(
    getTeacherSubjectAssignments
  )

  const form = useForm<
    z.input<typeof assignmentSchema>,
    unknown,
    z.output<typeof assignmentSchema>
  >({
    resolver: zodResolver(assignmentSchema),
    defaultValues: assignment
      ? {
          title: assignment.title,
          description: assignment.description,
          deadline: toDatetimeLocalValue(assignment.deadline),
          maxMarks: assignment.maxMarks,
          allowResubmission: assignment.allowResubmission,
        }
      : {
          title: "",
          description: "",
          deadline: "",
          maxMarks: 100,
          allowResubmission: true,
        },
  })

  const [teacherSubjectAssignmentId, setTeacherSubjectAssignmentId] =
    useState("")
  const [teacherSubjectAssignmentError, setTeacherSubjectAssignmentError] =
    useState<string | null>(null)

  async function onSubmit(values: AssignmentFormValues) {
    if (!assignment && !teacherSubjectAssignmentId) {
      setTeacherSubjectAssignmentError("Class-Subject is required.")
      return
    }

    try {
      const payload = {
        title: values.title,
        description: values.description,
        deadline: fromDatetimeLocalValue(values.deadline),
        maxMarks: values.maxMarks,
        allowResubmission: values.allowResubmission,
      }

      if (assignment) {
        await updateAssignment(assignment.id, payload)
        toast.success("Assignment updated.")
      } else {
        await createAssignment({
          teacherSubjectAssignmentId,
          ...payload,
        })
        toast.success("Assignment created.")
      }
      onSaved()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to save assignment.")
        : "Failed to save assignment."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        {!assignment && (
          <div className="grid gap-2">
            <label className="text-sm leading-none font-medium">
              Class-Subject
            </label>
            <Select
              value={teacherSubjectAssignmentId}
              onValueChange={(value: string | null) => {
                setTeacherSubjectAssignmentId(value ?? "")
                setTeacherSubjectAssignmentError(null)
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Select a class-subject">
                  {(value: string | null) => {
                    if (!value) return "Select a class-subject"
                    const selected = teacherSubjectAssignments.find(
                      (t) => t.id === value
                    )
                    return selected
                      ? `${selected.className} · ${selected.subjectName}`
                      : value
                  }}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {teacherSubjectAssignments.length === 0 && (
                  <div className="px-2 py-1.5 text-sm text-muted-foreground">
                    You haven't been assigned to a class-subject yet.
                  </div>
                )}
                {teacherSubjectAssignments.map((t) => (
                  <SelectItem key={t.id} value={t.id}>
                    {t.className} · {t.subjectName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {teacherSubjectAssignmentError && (
              <p className="text-destructive text-sm">
                {teacherSubjectAssignmentError}
              </p>
            )}
          </div>
        )}
        <FormField
          control={form.control}
          name="title"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Title</FormLabel>
              <FormControl>
                <Input placeholder="Homework 1" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea placeholder="Chapters 1-3" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="deadline"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Deadline</FormLabel>
              <FormControl>
                <Input type="datetime-local" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="maxMarks"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Max Marks</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min={1}
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
          name="allowResubmission"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center gap-2">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <FormLabel className="mb-0">Allow resubmission</FormLabel>
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting
            ? "Saving..."
            : assignment
              ? "Save Changes"
              : "Create"}
        </Button>
      </form>
    </Form>
  )
}
