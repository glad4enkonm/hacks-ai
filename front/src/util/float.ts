export function isFloat(val: string) {
  const floatRegex = /^-?\d+(?:[.,]\d*?)?$/;
  if (!floatRegex.test(val))
    return false;

  let result = parseFloat(val);
  if (isNaN(result))
    return false;
  return true;
}