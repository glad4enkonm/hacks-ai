import api from "@/api";
import type { IUser } from "@/model/user";


class UserApiService {
  static getInstance(): UserApiService {
    return this.instance || (this.instance = new this())
  }

  private static instance: UserApiService

  private baseUrl = '/user'

  private constructor() {
    // empty for linter
  }

  auth(payload: IUser) {
    const url = this.baseUrl + `/authenticate`
    return api.post<IUser>(url, payload)
  }

  getUser() {
    return api.get<IUser>(this.baseUrl)
  }

  getUserById(id: bigint) {
    return api.get<IUser>(this.baseUrl + `/${id}`)
  }

  registration(payload: IUser) {
    return api.post(this.baseUrl, payload)
  }

  update(payload: any) {
    return api.patch<IUser>(this.baseUrl, payload)
  }

  renewToken() {
    const url = this.baseUrl + `/refresh-token`
    return api.post<IUser>(url)
  }

  // Операции для User -------------------------------
  // Запросить данные
  getUserList() {
    return api.get<IUser[]>(this.baseUrl + '/user' + '/list')
  }

  // Создать данные
  createUser(payload: IUser) {
    return api.post<IUser>(this.baseUrl + '/user', payload)
  }

  // Обновить данные
  updateUser(payload: any) {
    return api.patch<IUser>(this.baseUrl + '/user', payload)
  }

  // Удалить данные
  deleteUser(id: bigint) {
    return api.delete(this.baseUrl + '/user' + '/' + id)
  }

}

export default UserApiService.getInstance()
