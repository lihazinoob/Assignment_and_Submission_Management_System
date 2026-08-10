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
import { createSubject, getSubjects } from "@/features/subjects/api"
import { useAsyncList } from "@/hooks/use-async-list"

const subjectSchema = z.object({
  name: z.string().min(1, "Name is required"),
  code: z.string().min(1, "Code is required"),
})

type SubjectFormValues = z.infer<typeof subjectSchema>

export function SubjectsPage() {
  const {
    data: subjects,
    isLoading,
    error,
    refetch,
  } = useAsyncList(getSubjects)

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Subjects</h1>
        <ResourceDialog triggerLabel="Add Subject" title="Add Subject">
          {(close) => (
            <SubjectForm
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
              <TableHead>Name</TableHead>
              <TableHead>Code</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {subjects.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} className="text-muted-foreground">
                  No subjects yet.
                </TableCell>
              </TableRow>
            )}
            {subjects.map((s) => (
              <TableRow key={s.id}>
                <TableCell>{s.name}</TableCell>
                <TableCell>{s.code}</TableCell>
                <TableCell>
                  <Badge variant={s.isActive ? "default" : "secondary"}>
                    {s.isActive ? "Active" : "Inactive"}
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

function SubjectForm({ onCreated }: { onCreated: () => void }) {
  const form = useForm<SubjectFormValues>({
    resolver: zodResolver(subjectSchema),
    defaultValues: { name: "", code: "" },
  })

  async function onSubmit(values: SubjectFormValues) {
    try {
      await createSubject(values)
      toast.success("Subject created.")
      onCreated()
    } catch (err) {
      const message = isAxiosError(err)
        ? (err.response?.data?.message ?? "Failed to create subject.")
        : "Failed to create subject."
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
                <Input placeholder="Mathematics" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="code"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Code</FormLabel>
              <FormControl>
                <Input placeholder="MATH101" {...field} />
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
