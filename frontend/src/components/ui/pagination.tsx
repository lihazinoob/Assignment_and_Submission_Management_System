import { ChevronLeft, ChevronRight } from "lucide-react"

import { Button } from "@/components/ui/button"

interface PaginationProps {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}

function Pagination({ page, totalPages, totalCount, onPageChange }: PaginationProps) {
  if (totalCount === 0) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-2 pt-2">
      <p className="text-sm text-muted-foreground">
        Page {page} of {totalPages} &middot; {totalCount} total
      </p>
      <div className="flex items-center gap-1">
        <Button
          variant="outline"
          size="icon-sm"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
          aria-label="Previous page"
        >
          <ChevronLeft />
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
          aria-label="Next page"
        >
          <ChevronRight />
        </Button>
      </div>
    </div>
  )
}

export { Pagination }
