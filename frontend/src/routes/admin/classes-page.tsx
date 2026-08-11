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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { ResourceDialog } from "@/components/resource-dialog"
import {
  activateClass,
  createClass,
  deactivateClass,
  deleteClass,
  getClasses,
  updateClass,
} from "@/features/classes/api"
import { useAsyncList } from "@/hooks/use-async-list"
import type { SchoolClass } from "@/types/class"

const classSchema = z.object({
  name: z.string().min(1, "Name is required"),
  academicYear: z.string().min(1, "Academic year is required"),
})

type ClassFormValues = z.infer<typeof classSchema>

export function ClassesPage() {
  const { data: classes, isLoading, error, refetch } = useAsyncList(getClasses)

  async function handleToggleActive(schoolClass: SchoolClass) {
    const action = schoolClass.isActive ? "deactivate" : "activate"
    if (
      !confirm(
        schoolClass.isActive
          ? `Deactivate ${schoolClass.name}? No new subjects, teacher assignments, enrollments, assignments, or submissions can be created for it until reactivated.`
          : `Reactivate ${schoolClass.name}?`
      )
    ) {
      return
    }

    try {
      if (schoolClass.isActive) {
        await deactivateClass(schoolClass.id)
      } else {
        await activateClass(schoolClass.id)
      }
      toast.success(`${schoolClass.name} ${action}d.`)
      refetch()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? `Failed to ${action} class.`)
        : `Failed to ${action} class.`
      toast.error(message)
    }
  }

  async function handleDelete(schoolClass: SchoolClass) {
    if (!confirm(`Delete ${schoolClass.name}? This cannot be undone.`)) return

    try {
      await deleteClass(schoolClass.id)
      toast.success("Class deleted.")
      refetch()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to delete class.")
        : "Failed to delete class."
      toast.error(message)
    }
  }

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Classes</h1>
        <ResourceDialog triggerLabel="Add Class" title="Add Class">
          {(close) => <ClassForm onSaved={() => { refetch(); close() }} />}
        </ResourceDialog>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading...</p>}
      {error && <p className="text-destructive">{error}</p>}

      {!isLoading && !error && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Academic Year</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {classes.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-muted-foreground">
                  No classes yet.
                </TableCell>
              </TableRow>
            )}
            {classes.map((c) => (
              <TableRow key={c.id}>
                <TableCell>{c.name}</TableCell>
                <TableCell>{c.academicYear}</TableCell>
                <TableCell>
                  <Badge variant={c.isActive ? "default" : "secondary"}>
                    {c.isActive ? "Active" : "Inactive"}
                  </Badge>
                </TableCell>
                <TableCell>
                  <div className="flex gap-2">
                    <ResourceDialog
                      triggerLabel="Edit"
                      triggerVariant="outline"
                      triggerSize="sm"
                      title="Edit Class"
                    >
                      {(close) => (
                        <ClassForm
                          schoolClass={c}
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
                      onClick={() => handleToggleActive(c)}
                    >
                      {c.isActive ? "Deactivate" : "Activate"}
                    </Button>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleDelete(c)}
                    >
                      Delete
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function ClassForm({
  schoolClass,
  onSaved,
}: {
  schoolClass?: SchoolClass
  onSaved: () => void
}) {
  const form = useForm<ClassFormValues>({
    resolver: zodResolver(classSchema),
    defaultValues: {
      name: schoolClass?.name ?? "",
      academicYear: schoolClass?.academicYear ?? "",
    },
  })

  async function onSubmit(values: ClassFormValues) {
    try {
      if (schoolClass) {
        await updateClass(schoolClass.id, values)
        toast.success("Class updated.")
      } else {
        await createClass(values)
        toast.success("Class created.")
      }
      onSaved()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to save class.")
        : "Failed to save class."
      toast.error(message)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="grid gap-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input placeholder="Grade 10 - Section A" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="academicYear"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Academic Year</FormLabel>
              <FormControl>
                <Input placeholder="2026" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting
            ? "Saving..."
            : schoolClass
              ? "Save Changes"
              : "Create"}
        </Button>
      </form>
    </Form>
  )
}
