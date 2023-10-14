import { defineStore } from "pinia";
import type { AxiosResponse } from "axios";

export const useErrorStore = defineStore({
  id: "error",
  state: () => ({
    apiError: '',
  }),
  actions: {
    setErrorMessageIfAny(response: AxiosResponse<any, any>) {
      if (response.status !== 200) {
        this.apiError = response.data.message || response.data
      }
    },
  },
});
