import { useCallback, useEffect, useState } from "react"

export function useAsyncList<T>(fetcher: () => Promise<T[]>) {
  const [data, setData] = useState<T[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const refetch = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      setData(await fetcher())
    } catch {
      setError("Failed to load data.")
    } finally {
      setIsLoading(false)
    }
  }, [fetcher])

  useEffect(() => {
    // Fetch-on-mount is an intentional two-render pattern; this project has no
    // data-fetching library (react-query etc.) to avoid it.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refetch()
  }, [refetch])

  return { data, isLoading, error, refetch }
}
