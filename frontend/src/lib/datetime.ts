export function toDatetimeLocalValue(iso: string): string {
  const date = new Date(iso)
  const offsetMs = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

export function fromDatetimeLocalValue(value: string): string {
  return new Date(value).toISOString()
}
