// Запросить данные базовая операция
import type { AxiosResponse } from "axios";
import type { ICanAccessPropByString, IEntityDictionary } from "./dictionary";
import { useErrorStore } from "@/stores/error";
import { propIntersectionShallowObjectDiff } from "@/util/diff";

// Запросить список данных базовая операция
export const loadElementListBase = async function<T>(
  getList: () => Promise<AxiosResponse<T[], any>>,
  getId: (entity: T) => (bigint | undefined),
  setStoreData: (entityDictionary: IEntityDictionary<T>, originalList: T[]) => void
) {
  const result = await getList()
  useErrorStore().setErrorMessageIfAny(result)
  if (result.status != 200) return;
  const newElementDictionary = {} as IEntityDictionary<T>
  for (const entity of result.data) {
    const entityId =getId(entity)
    if ( entityId == undefined) throw new Error("id должен быть определён")
    newElementDictionary[entityId.toString()] = entity
  }
  setStoreData(newElementDictionary, result.data)
}

// Запросить элемент данных базовая операция
export const loadElementBase = async function<T>(id: bigint,
  get: (id: bigint) => Promise<AxiosResponse<T, any>>,
  getId: (entity: T) => (bigint | undefined),
  setStoreData: (id: string, entity: T) => void
) {
  const result = await get(id)
  useErrorStore().setErrorMessageIfAny(result)
  if (result.status != 200) return;
  const retrievedEntity = result.data, retrievedEntityId = getId(retrievedEntity)
  if (!retrievedEntity || !retrievedEntityId) throw new Error("ожидали получить данные")
  setStoreData(retrievedEntityId.toString(), retrievedEntity)
  return retrievedEntity
}

// Создать элемент данных базовая операция
export const createElementBase = async function<T>(payload: T,
  create: (payload: T) => Promise<AxiosResponse<T, any>>,
  getId: (entity: T) => (bigint | undefined),
  setStoreData: (id: string, entity: T) => void
) {
  const result = await create(payload)
  useErrorStore().setErrorMessageIfAny(result)
  if (result.status != 200) return
  const newEntity = result.data, newEntityId = getId(newEntity)
  if (!newEntity || !newEntityId) throw new Error("ожидали получить данные")
  setStoreData(newEntityId.toString(), newEntity)

  return newEntity
}

// Обновить элемент данных базовая операция
export const updateElementBase = async function<T>(updatedEntity:  T,
  update: (payload: any) => Promise<AxiosResponse<T, any> | null> ,
  getId: (entity: T) => (bigint | undefined),
  getStoreData: (id: string) => T,
  setStoreData: (id: string, entity: T) => void,
  entityIdPropertyName: string
) {
  const updatedEntityId = getId(updatedEntity)
  if (updatedEntityId == undefined) throw new Error("id должен быть определён")
  const entityInStore = getStoreData(updatedEntityId.toString())
  const entityDiff = propIntersectionShallowObjectDiff(entityInStore, updatedEntity) as ICanAccessPropByString
  if (Object.keys(entityDiff).length == 0) return // нет изменений и не вызывали REST
  entityDiff[entityIdPropertyName] = getId(updatedEntity) // добавляем id для определения объекта в БД
  const updateResponse = await update(entityDiff)
  if (updateResponse == null) return // нет изменений и не вызывали REST // TODO: проверить нужно ли дважды
  useErrorStore().setErrorMessageIfAny(updateResponse)
  if (updateResponse?.status != 200) return

  const retrievedEntity = updateResponse.data, retrievedEntityId = getId(retrievedEntity)
  if (!retrievedEntity || !retrievedEntityId) throw new Error("ожидали получить данные")
  setStoreData(retrievedEntityId.toString(), retrievedEntity)
  return retrievedEntity
}


// Удалить элемент данных базовая операция
export const deleteElementBase = async function<T>(id: bigint,
  del: (id: bigint) => Promise<AxiosResponse<any, any>>,
  getStoreData: (id: string) => T,
  setStoreData: (id: string) => void
) {
  const result = await del(id)
  useErrorStore().setErrorMessageIfAny(result)
  if (result.status != 200) return
  const entityFromStore = getStoreData(id.toString())
  if (!entityFromStore) throw new Error("ожидали присутствие элемента")
  setStoreData(id.toString())
  return result.data
}