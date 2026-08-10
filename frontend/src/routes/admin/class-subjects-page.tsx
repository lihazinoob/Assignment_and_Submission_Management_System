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
import { getClasses } from "@/features/classes/api"
import { createClassSubject, getClassSubjects } from "@/features/class-subjects/api"
import { getSubjects } from "@/features/subjects/api"
import { useAsyncList } from "@/hooks/use-async-list"

const classSubjectSchema = z.object({
  classId: z.string().min(1, "Class is required"),
  subjectId: z.string().min(1, "Subject is required"),
})

type ClassSubjectFormValues = z.infer<typeof classSubjectSchema>

export function ClassSubjectsPage() {
  const {
    data: classSubjects,
    isLoading,
    error,
    refetch,
  } = useAsyncList(getClassSubjects)

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Class-Subjects</h1>
        <ResourceDialog triggerLabel="Add Link" title="Link Class to Subject">
          {(close) => (
            <ClassSubjectForm
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
              <TableHead>Class</TableHead>
              <TableHead>Subject</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {classSubjects.length === 0 && (
              <TableRow>
                <TableCell colSpan={2} className="text-muted-foreground">
                  No class-subject links yet.
                </TableCell>
              </TableRow>
            )}
            {classSubjects.map((cs) => (
              <TableRow key={cs.id}>
                <TableCell>{cs.className}</TableCell>
                <TableCell>{cs.subjectName}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function ClassSubjectForm({ onCreated }: { onCreated: () => void }) {
  const { data: classes } = useAsyncList(getClasses)
  const { data: subjects } = useAsyncList(getSubjects)

  const form = useForm<ClassSubjectFormValues>({
    resolver: zodResolver(classSubjectSchema),
    defaultValues: { classId: "", subjectId: "" },
  })

  async function onSubmit(values: ClassSubjectFormValues) {
    try {
      await createClassSubject(values)
      toast.success("Class linked to subject.")
      onCreated()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to create link.")
        : "Failed to create link."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
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
          name="subjectId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Subject</FormLabel>
              <Select value={field.value} onValueChange={field.onChange}>
                <FormControl>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select a subject">
                      {(value: string | null) => {
                        if (!value) return "Select a subject"
                        const selected = subjects.find((s) => s.id === value)
                        return selected
                          ? `${selected.name} (${selected.code})`
                          : value
                      }}
                    </SelectValue>
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {subjects.map((s) => (
                    <SelectItem key={s.id} value={s.id}>
                      {s.name} ({s.code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting ? "Linking..." : "Link"}
        </Button>
      </form>
    </Form>
  )
}
