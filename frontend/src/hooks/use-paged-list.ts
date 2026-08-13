import { useCallback, useEffect, useRef, useState } from "react"

import type { PagedResponse } from "@/types/api"

export function usePagedList<T, F extends object>(
  fetcher: (page: number, pageSize: number, filters: F) => Promise<PagedResponse<T>>,
  filters: F,
  pageSize = 10
) {
  const [page, setPage] = useState(1)
  const [data, setData] = useState<T[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const filtersKey = JSON.stringify(filters)
  const previousFiltersKey = useRef(filtersKey)

  const refetch = useCallback(async () => {
    if (previousFiltersKey.current !== filtersKey && page !== 1) {
      previousFiltersKey.current = filtersKey
      setPage(1)
      return
    }
    previousFiltersKey.current = filtersKey

    setIsLoading(true)
    setError(null)
    try {
      const result = await fetcher(page, pageSize, filters)
      setData(result.items)
      setTotalCount(result.totalCount)
    } catch {
      setError("Failed to load data.")
    } finally {
      setIsLoading(false)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fetcher, page, pageSize, filtersKey])

  useEffect(() => {
    // Fetch-on-mount is an intentional two-render pattern; this project has no
    // data-fetching library (react-query etc.) to avoid it.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refetch()
  }, [refetch])

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return { data, totalCount, totalPages, page, setPage, isLoading, error, refetch }
}
