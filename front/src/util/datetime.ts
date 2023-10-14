export function formatDateTime(utcTimestamp: Date): string {
  const date = new Date(utcTimestamp);
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60 * 1000);
  return `${localDate.getDate()}.${localDate.getMonth() + 1}.${localDate.getFullYear()} `
    +`${localDate.getHours()}:${localDate.getMinutes()}:${localDate.getSeconds()}`

}