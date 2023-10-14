import clone from "just-clone";

interface TextIndexed {
  [thingName: string]: any
}

export function propIntersectionShallowObjectDiff(o1: TextIndexed, o2: TextIndexed): object {
  const keys1 = new Set(Object.keys(o1)), keys2 = new Set(Object.keys(o2))
  const keysIntersection: string[] = Array.from(intersection(keys1, keys2))
  const o1propIntersectionReduced = reduceObjectToAllowedProps(o1, keysIntersection)
  const o2propIntersectionReduced = reduceObjectToAllowedProps(o2, keysIntersection)
  return shallowObjectDiff(o1propIntersectionReduced, o2propIntersectionReduced)
}

function shallowObjectDiff(o1: TextIndexed, o2: TextIndexed): object {
  return Object.keys(o2).reduce((diff, key) => {
    if (o1[key] === o2[key]) return diff
    return {
      ...diff,
      [key]: o2[key]
    }
  }, {})
}

function intersection(setA: Set<string>, setB: Set<string>):Set<string> {
  const _intersection = new Set<string>();
  for (const elem of setB) {
    if (setA.has(elem)) {
      _intersection.add(elem);
    }
  }
  return _intersection;
}

function reduceObjectToAllowedProps(rawObject: TextIndexed, allowedProps: string[]) {
  return Object.keys(rawObject)
    .filter(key => allowedProps.includes(key))
    .reduce((obj: TextIndexed, key) => {
      obj[key] = rawObject[key];
      return obj;
    }, {})
}

export function setPropValuesIfDefinedInBothObjects(objectToSetProps:any, objectToGetProps: any) {
  for (let prop in objectToGetProps) { // проходим по свойствам, чтобы сохранить отображение
    if (!objectToGetProps.hasOwnProperty(prop) || !objectToSetProps.hasOwnProperty(prop))
      continue;
    objectToSetProps[prop] = objectToGetProps[prop];
  }
}

export function deepCloneAndReassignProps(thisSourceObject:any, propsList: string[]) {
  for (const prop of propsList)
    thisSourceObject[prop] = clone(thisSourceObject[prop])
}

export function deepClone(objectToClone: any) {
  return clone(objectToClone)
}
