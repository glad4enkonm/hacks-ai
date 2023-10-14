import { defineStore } from "pinia";
import { localStorageGet, localStorageSet } from "@/util/localStorage";
import type { IUser } from "@/model/user";
import userApiService from "@/api/user"
import { useErrorStore } from "@/stores/error";
import { propIntersectionShallowObjectDiff } from "@/util/diff";
import type { IEntityDictionary } from "@/util/dictionary";
import {
  createElementBase,
  deleteElementBase,
  loadElementBase,
  loadElementListBase,
  updateElementBase
} from "@/util/store";

import router from "@/router"

const adminUserId: any = 1
const logoutTimeInMinutes = 20
export const useUserStore = defineStore({
  id: "user",
  state: () => ({
    token: localStorageGet('token', true, '') as string,
    isRegistrationRequestSent: localStorageGet('isRegistrationRequestSent', true, false) == 'true',
    isRefreshTokenResponseReceived: false,
    isRefreshTokenRequested: false,
    user: {} as IUser,
    userDictionary: {} as IEntityDictionary<IUser>,
    userList: [] as IUser[],
    userLogoutTimer: null as any,
  }),
  actions: {
    async auth(user:IUser) {
      const response = await userApiService.auth(user)
      useErrorStore().setErrorMessageIfAny(response)
      this.token = response.data.token?? ""
      if (this.token.length > 0)
        localStorageSet('token', this.token)

    },
    async getUser() {
      const response = await userApiService.getUser()
      useErrorStore().setErrorMessageIfAny(response)
      if (response.status == 200) {
        this.user = response.data
        // Если отчет не установлен и пользователь не admin
        if (this.userLogoutTimer == null && !this.isCurrentUserAdmin()) {
          console.log("Timer set")
          this.userLogoutTimer = setTimeout(this.logout, logoutTimeInMinutes * 60 * 1000)
        }
      }
    },

    async logout() {
      this.token = ''
      localStorageSet('token', '')
      await router.push({ name: "auth" })
    },
    async register(user:IUser) {
      const tokenResponse = await userApiService.registration(user)
      useErrorStore().setErrorMessageIfAny(tokenResponse)
    },
    /*
    async updateUser(newUserObject: IUser) {
      const userDiff = propIntersectionShallowObjectDiff(this.user, newUserObject)

      const updateResponse = await userApiService.update(userDiff)
      if (updateResponse == null)
        return
      useErrorStore().setErrorMessageIfAny(updateResponse)
      if (updateResponse.status == 200) {
        this.user = updateResponse.data
      }
    }, */
    async renewToken() {
      this.isRefreshTokenResponseReceived = false
      const response = await userApiService.renewToken()
      this.isRefreshTokenResponseReceived = true
      if (response.status == 200) {
        this.token = response.data.token ?? ""
        localStorageSet('token', this.token)
      }
      useErrorStore().setErrorMessageIfAny(response)
    },
    setRegistrationRequestSent() {
      localStorageSet('isRegistrationRequestSent', 'true')
      this.isRegistrationRequestSent = true
    },
    setRefreshTokenRequested() {
      this.isRefreshTokenRequested = true
    },
    isCurrentUserAdmin() {
      return this.user.userId == adminUserId
    },

    // Операции для User -------------------------------
    // Запросить данные
    async loadUserList() {
      await loadElementListBase(() => userApiService.getUserList(),
        entity => entity.userId,
        (newDictionary, originalList) => {
          this.userDictionary = newDictionary
          this.userList = originalList
        })
    },
    // Создать данные
    async createUser(payload:  IUser) {
      const that = this
      return await createElementBase(payload, p =>  userApiService.createUser(p),
        entity => entity.userId,
        function(id, entity) {
          that.userDictionary[id] = entity
          that.userList.push(entity)
        })
    },
    // Обновить данные
    async updateUser(updatedEntity:  IUser) {
      const that = this
      return await updateElementBase(updatedEntity, diff => userApiService.updateUser(diff),
        entity => entity.userId, id => this.userDictionary[id],
        function(id, entity) {
          that.userDictionary[id] = entity
          that.userList = that.userList.map(e => e.userId?.toString() == id ? entity : e)
        },
        "userId")
    },
    // Удалить данные
    async deleteUser(id: bigint) {
      const that = this
      return await deleteElementBase<IUser>(id,
        id => userApiService.deleteUser(id),
        id => this.userDictionary[id],
        function(id) {
          delete that.userDictionary[id]
          that.userList = that.userList.filter(e => e.userId?.toString() != id)
        })
    },
  },
});
