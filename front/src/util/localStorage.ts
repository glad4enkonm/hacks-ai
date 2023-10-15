const PREFIX = 'hackII_'

export const localStorageGetMap = function(
  ...items: string[]
): Record<string, string> {
  const result: Record<string, string> = {}
  items.forEach(key => {
    let val: string | null = window.localStorage.getItem(PREFIX + key)
    if (val === null) {
      val = ''
    }
    result[key] = val
  })
  return result
}

export const localStorageGet = function(
  key: string,
  allowNotFound = false,
  defaultValue: any = ''
): string | null {
  const val: string | null = window.localStorage.getItem(PREFIX + key)
  if (val === null) {
    if (allowNotFound) return defaultValue
    throw `value with the key ${key} not found`
  }
  return val
}

export const localStorageSet = function(key: string, value: string): void {
  window.localStorage.setItem(PREFIX + key, value)
}
