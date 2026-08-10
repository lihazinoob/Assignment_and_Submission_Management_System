import { zodResolver } from "@hookform/resolvers/zod"
import { isAxiosError } from "axios"
import { useForm } from "react-hook-form"
import { toast } from "sonner"
import { z } from "zod"

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
import { ResourceDialog } from "@/components/resource-dialog"
import { getClassSubjects } from "@/features/class-subjects/api"
import {
  createTeacherSubjectAssignment,
  getTeacherSubjectAssignments,
} from "@/features/teacher-subject-assignments/api"
import { getTeachers } from "@/features/users/api"
import { useAsyncList } from "@/hooks/use-async-list"

const assignmentSchema = z.object({
  teacherId: z.string().min(1, "Teacher is required"),
  classSubjectId: z.string().min(1, "Class-Subject is required"),
})

type AssignmentFormValues = z.infer<typeof assignmentSchema>

export function TeacherAssignmentsPage() {
  const {
    data: assignments,
    isLoading,
    error,
    refetch,
  } = useAsyncList(getTeacherSubjectAssignments)

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Teacher Assignments</h1>
        <ResourceDialog triggerLabel="Add Assignment" title="Assign Teacher">
          {(close) => (
            <AssignmentForm
              onCreated={() => {
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
              <TableHead>Teacher</TableHead>
              <TableHead>Class</TableHead>
              <TableHead>Subject</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {assignments.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} className="text-muted-foreground">
                  No teacher assignments yet.
                </TableCell>
              </TableRow>
            )}
            {assignments.map((a) => (
              <TableRow key={a.id}>
                <TableCell>{a.teacherName}</TableCell>
                <TableCell>{a.className}</TableCell>
                <TableCell>{a.subjectName}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function AssignmentForm({ onCreated }: { onCreated: () => void }) {
  const { data: teachers } = useAsyncList(getTeachers)
  const { data: classSubjects } = useAsyncList(getClassSubjects)

  const form = useForm<AssignmentFormValues>({
    resolver: zodResolver(assignmentSchema),
    defaultValues: { teacherId: "", classSubjectId: "" },
  })

  async function onSubmit(values: AssignmentFormValues) {
    try {
      await createTeacherSubjectAssignment(values)
      toast.success("Teacher assigned.")
      onCreated()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to assign teacher.")
        : "Failed to assign teacher."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        <FormField
          control={form.control}
          name="teacherId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Teacher</FormLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select a teacher">
                      {(value: string | null) => {
                        if (!value) return "Select a teacher"
                        const selected = teachers.find((t) => t.id === value)
                        return selected ? selected.fullName : value
                      }}
                    </SelectValue>
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {teachers.length === 0 && (
                    <div className="px-2 py-1.5 text-sm text-muted-foreground">
                      No teachers yet — create one on the Users page.
                    </div>
                  )}
                  {teachers.map((t) => (
                    <SelectItem key={t.id} value={t.id}>
                      {t.fullName} ({t.email})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="classSubjectId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Class-Subject</FormLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select a class-subject">
                      {(value: string | null) => {
                        if (!value) return "Select a class-subject"
                        const selected = classSubjects.find(
                          (cs) => cs.id === value
                        )
                        return selected
                          ? `${selected.className} · ${selected.subjectName}`
                          : value
                      }}
                    </SelectValue>
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {classSubjects.length === 0 && (
                    <div className="px-2 py-1.5 text-sm text-muted-foreground">
                      No class-subject links yet — create one first.
                    </div>
                  )}
                  {classSubjects.map((cs) => (
                    <SelectItem key={cs.id} value={cs.id}>
                      {cs.className} · {cs.subjectName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting ? "Assigning..." : "Assign"}
        </Button>
      </form>
    </Form>
  )
}
