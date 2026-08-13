import { useMemo, useState } from "react"

import { Badge } from "@/components/ui/badge"
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
import { getAssignmentsPaged } from "@/features/assignments/api"
import { usePagedList } from "@/hooks/use-paged-list"
import type { Assignment, AssignmentStatus } from "@/types/assignment"

const STATUS_OPTIONS = ["all", "Draft", "Published"] as const
type StatusOption = (typeof STATUS_OPTIONS)[number]

const STATUS_LABELS: Record<StatusOption, string> = {
  all: "All statuses",
  Draft: "Draft",
  Published: "Published",
}

export function AssignmentsPage() {
  const [status, setStatus] = useState<StatusOption>("all")
  const [search, setSearch] = useState("")

  const filters = useMemo(
    () => ({
      status: status === "all" ? undefined : (status as AssignmentStatus),
      search: search.trim() || undefined,
    }),
    [status, search]
  )

  const fetcher = useMemo(
    () => (page: number, pageSize: number, f: typeof filters) =>
      getAssignmentsPaged(page, pageSize, f),
    []
  )

  const {
    data: assignments,
    totalCount,
    totalPages,
    page,
    setPage,
    isLoading,
    error,
  } = usePagedList(fetcher, filters, 10)

  return (
    <div className="grid gap-4">
      <h1 className="text-2xl font-semibold">Assignments</h1>

      <div className="flex flex-wrap items-end gap-3">
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
                  STATUS_LABELS[(value as StatusOption) ?? "all"]
                }
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              {STATUS_OPTIONS.map((option) => (
                <SelectItem key={option} value={option}>
                  {STATUS_LABELS[option]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="grid gap-2">
          <label className="text-sm leading-none font-medium">Search</label>
          <Input
            className="w-56"
            placeholder="Search by title..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading...</p>}
      {error && <p className="text-destructive">{error}</p>}

      {!isLoading && !error && (
        <>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Title</TableHead>
                <TableHead>Teacher</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Subject</TableHead>
                <TableHead>Deadline</TableHead>
                <TableHead>Max Marks</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Details</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {assignments.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8} className="text-muted-foreground">
                    No assignments found.
                  </TableCell>
                </TableRow>
              )}
              {assignments.map((a) => (
                <TableRow key={a.id}>
                  <TableCell>{a.title}</TableCell>
                  <TableCell>{a.teacherName}</TableCell>
                  <TableCell>{a.className}</TableCell>
                  <TableCell>{a.subjectName}</TableCell>
                  <TableCell>
                    {new Date(a.deadline).toLocaleString()}
                  </TableCell>
                  <TableCell>{a.maxMarks}</TableCell>
                  <TableCell>
                    <Badge variant={a.status === "Published" ? "default" : "secondary"}>
                      {a.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <ResourceDialog
                      triggerLabel="View"
                      triggerVariant="outline"
                      triggerSize="sm"
                      title={a.title}
                    >
                      {() => <AssignmentDetails assignment={a} />}
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

function AssignmentDetails({ assignment }: { assignment: Assignment }) {
  return (
    <dl className="grid gap-3 text-sm">
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Teacher</dt>
        <dd className="col-span-2">{assignment.teacherName}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Class</dt>
        <dd className="col-span-2">{assignment.className}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Subject</dt>
        <dd className="col-span-2">{assignment.subjectName}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Description</dt>
        <dd className="col-span-2 whitespace-pre-wrap">
          {assignment.description || "—"}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Deadline</dt>
        <dd className="col-span-2">
          {new Date(assignment.deadline).toLocaleString()}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Max Marks</dt>
        <dd className="col-span-2">{assignment.maxMarks}</dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Allow Resubmission</dt>
        <dd className="col-span-2">
          {assignment.allowResubmission ? "Yes" : "No"}
        </dd>
      </div>
      <div className="grid grid-cols-3 gap-2">
        <dt className="text-muted-foreground">Status</dt>
        <dd className="col-span-2">
          <Badge variant={assignment.status === "Published" ? "default" : "secondary"}>
            {assignment.status}
          </Badge>
        </dd>
      </div>
    </dl>
  )
}
