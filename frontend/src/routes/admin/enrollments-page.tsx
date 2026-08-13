import { zodResolver } from "@hookform/resolvers/zod"
import { isAxiosError } from "axios"
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
import { ResourceDialog } from "@/components/resource-dialog"
import { getClasses } from "@/features/classes/api"
import {
  createStudentEnrollment,
  getStudentEnrollmentsPaged,
} from "@/features/student-enrollments/api"
import { getStudents } from "@/features/users/api"
import { useAsyncList } from "@/hooks/use-async-list"
import { usePagedList } from "@/hooks/use-paged-list"

const emptyFilters = {}

async function fetchEnrollments(page: number, pageSize: number) {
  return getStudentEnrollmentsPaged(page, pageSize)
}

const enrollmentSchema = z.object({
  studentId: z.string().min(1, "Student is required"),
  classId: z.string().min(1, "Class is required"),
  rollNumber: z.string().optional(),
})

type EnrollmentFormValues = z.infer<typeof enrollmentSchema>

export function EnrollmentsPage() {
  const {
    data: enrollments,
    totalCount,
    totalPages,
    page,
    setPage,
    isLoading,
    error,
    refetch,
  } = usePagedList(fetchEnrollments, emptyFilters, 10)

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Enrollments</h1>
        <ResourceDialog triggerLabel="Add Enrollment" title="Enroll Student">
          {(close) => (
            <EnrollmentForm
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
        <>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Student</TableHead>
              <TableHead>Class</TableHead>
              <TableHead>Roll Number</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {enrollments.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-muted-foreground">
                  No enrollments yet.
                </TableCell>
              </TableRow>
            )}
            {enrollments.map((e) => (
              <TableRow key={e.id}>
                <TableCell>{e.studentName}</TableCell>
                <TableCell>{e.className}</TableCell>
                <TableCell>{e.rollNumber ?? "—"}</TableCell>
                <TableCell>
                  <Badge
                    variant={e.status === "Active" ? "default" : "secondary"}
                  >
                    {e.status}
                  </Badge>
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

function EnrollmentForm({ onCreated }: { onCreated: () => void }) {
  const { data: students } = useAsyncList(getStudents)
  const { data: classes } = useAsyncList(getClasses)

  const form = useForm<EnrollmentFormValues>({
    resolver: zodResolver(enrollmentSchema),
    defaultValues: { studentId: "", classId: "", rollNumber: "" },
  })

  async function onSubmit(values: EnrollmentFormValues) {
    try {
      await createStudentEnrollment({
        studentId: values.studentId,
        classId: values.classId,
        rollNumber: values.rollNumber || undefined,
      })
      toast.success("Student enrolled.")
      onCreated()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to enroll student.")
        : "Failed to enroll student."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        <FormField
          control={form.control}
          name="studentId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Student</FormLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select a student">
                      {(value: string | null) => {
                        if (!value) return "Select a student"
                        const selected = students.find((s) => s.id === value)
                        return selected ? selected.fullName : value
                      }}
                    </SelectValue>
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {students.length === 0 && (
                    <div className="px-2 py-1.5 text-sm text-muted-foreground">
                      No students yet — create one on the Users page.
                    </div>
                  )}
                  {students.map((s) => (
                    <SelectItem key={s.id} value={s.id}>
                      {s.fullName} ({s.email})
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
          name="classId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Class</FormLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select a class">
                      {(value: string | null) => {
                        if (!value) return "Select a class"
                        const selected = classes.find((c) => c.id === value)
                        return selected
                          ? `${selected.name} (${selected.academicYear})`
                          : value
                      }}
                    </SelectValue>
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {classes.map((c) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.name} ({c.academicYear})
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
          name="rollNumber"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Roll Number (optional)</FormLabel>
              <FormControl>
                <Input placeholder="R-01" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting ? "Enrolling..." : "Enroll"}
        </Button>
      </form>
    </Form>
  )
}
