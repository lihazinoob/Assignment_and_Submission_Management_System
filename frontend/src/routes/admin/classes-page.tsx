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
import { createClass, getClasses } from "@/features/classes/api"
import { useAsyncList } from "@/hooks/use-async-list"

const classSchema = z.object({
  name: z.string().min(1, "Name is required"),
  academicYear: z.string().min(1, "Academic year is required"),
})

type ClassFormValues = z.infer<typeof classSchema>

export function ClassesPage() {
  const { data: classes, isLoading, error, refetch } = useAsyncList(getClasses)

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Classes</h1>
        <ResourceDialog triggerLabel="Add Class" title="Add Class">
          {(close) => <ClassForm onCreated={() => { refetch(); close() }} />}
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
            </TableRow>
          </TableHeader>
          <TableBody>
            {classes.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} className="text-muted-foreground">
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
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  )
}

function ClassForm({ onCreated }: { onCreated: () => void }) {
  const form = useForm<ClassFormValues>({
    resolver: zodResolver(classSchema),
    defaultValues: { name: "", academicYear: "" },
  })

  async function onSubmit(values: ClassFormValues) {
    try {
      await createClass(values)
      toast.success("Class created.")
      onCreated()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to create class.")
        : "Failed to create class."
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
          {form.formState.isSubmitting ? "Creating..." : "Create"}
        </Button>
      </form>
    </Form>
  )
}
