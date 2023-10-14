export interface IEntityDictionary<T> {
  [id: string]: T;
}

export interface ICanAccessPropByString {
  [id: string]: any;
}

export function applyFunctionToListObjects(
  keyProp: string ,list: any[], that: any,
  transformFunction: (item:any, that: any) => any): ICanAccessPropByString
{
  const resultObj = {} as ICanAccessPropByString
  list.forEach((item) => {
    resultObj[item[keyProp]] = transformFunction(item, that)
  })
  return resultObj
}
