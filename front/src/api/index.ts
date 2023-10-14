import axios from "axios";
import type { AxiosError, AxiosInstance, AxiosResponse } from "axios"
import { useUserStore } from "@/stores/user"
import router from "@/router"

import { useToast } from 'vue-toastification'
import { firstLetterToUpperCase } from "@/util/naming";

const toast = useToast()

export type ApiError = AxiosError<{
  status: number
  code: number
  message: string
}>

class ApiService {
  private static _instance: ApiService
  private baseURL = '/api' // TODO: add /api 'http://localhost:5000/'
  private axiosInstance: AxiosInstance = axios.create({
    baseURL: this.baseURL,
    headers: {
      'Content-Type': 'application/json; charset=utf-8',
    },
  })

  private constructor() {
    this.axiosInstance.interceptors.request.use(config => {
      const token = useUserStore().token
      if (token.length > 0 && config.headers !== undefined) {
        config.headers['Authorization'] = token
      }

      return config
    })

    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse<unknown>): AxiosResponse<unknown> => {
        return response
      },
      async (error: AxiosError<ApiError>): Promise<ApiError> => {
        const status = error.response ? error.response.status : null
        const userStore = useUserStore()
        if (status === 401 && userStore.isRefreshTokenResponseReceived == false) {
          userStore.token = ''
          if (! userStore.isRefreshTokenRequested) {
            userStore.setRefreshTokenRequested()
            await userStore.renewToken()
          }
        } else if (status === 401 && userStore.isRefreshTokenResponseReceived ) {
          await router.push({ name: "auth" })
        } else {
          toast.error('' + (error.response?.data?.message || error.response?.data))
        }

        console.error(error.response?.data)
        return Promise.reject(error.response?.data)
      },
    )
  }

  private patchDataTransformer(payload: any):any[] | null {
    const payloadArray = [] as object[]
    let anyPropInResult = false
    for (const property in payload) {
      let value:string = payload[property] instanceof Date ?
        payload[property].toISOString() : payload[property].toString()
      payloadArray.push({Key: firstLetterToUpperCase(property), Value: value}) // на сервере свойства с большой буквы
      anyPropInResult = true
    }
    return anyPropInResult ? payloadArray : null
  }

  private patchWithDataTransform<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D): Promise<R|null> {
    const transformedData = this.patchDataTransformer(data)
    if (transformedData == null)
      return Promise.resolve(null)
    return this.axiosInstance.patch(url, transformedData);
  }

  static getInstance(): ApiService {
    return this._instance || (this._instance = new this())
  }

  public get = this.axiosInstance.get
  public post = this.axiosInstance.post
  public put = this.axiosInstance.put
  public delete = this.axiosInstance.delete
  public patch = this.patchWithDataTransform
}

export default ApiService.getInstance()
