<script setup lang="ts">
import Sidebar from "@/components/Sidebar.vue";
</script>

<script lang="ts">
import { defineComponent } from "vue";

import { useToast } from 'vue-toastification'
import { useErrorStore } from "@/stores/error";
import { deepClone, setPropValuesIfDefinedInBothObjects } from "@/util/diff";
import { customerStore } from "@/stores/customer";
import type { ICode } from "@/model/code";
import { useUserStore } from "@/stores/user";
import ru from "@/util/text";
import { getRandomNumber } from "@/util/random";
import { isFloat } from "@/util/float";
import { formatDateTime } from "@/util/datetime";
import type { IUser } from "@/model/user";
import { hash } from "@/util/hash";
const toast = useToast()

import type { RenderConfigField, RenderConfig } from "@/util/renderConfig";

export default defineComponent({
  data() {
    return {
      error: '',
      isEdit: false,
      userStore: useUserStore(),
      errorStore: useErrorStore(),
      userObject: {
        login: "",
        name: "",
        passwordHash: "",
        description: "",
      },

    }
  },
  methods: {
    async createUser() {
      if (!this.validateCodeObject())
        return;

      const transportUser = { ... this.userObject} as any
      transportUser.passwordHash = await hash(transportUser.passwordHash)

      await this.userStore.createUser(transportUser)
      toast.success(ru.userCreate.createdMsg)

      this.$router.push({name: "user"})
    },
    async editUser() {
      if (!this.validateCodeObject())
        return;

      if (this.userObject.passwordHash.length > 0)
        this.userObject.passwordHash = await hash(this.userObject.passwordHash)

      await this.userStore.updateUser(this.userObject as any)
      toast.success(ru.userCreate.updatedMsg)
      this.$router.push({name: "user"})
    },
    isValidPassword(password:string):boolean {
      let regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&])[a-zA-Z\d@$!%*#?&]{10,}$/
      return regex.test(password)
    },
    validateCodeObject(): boolean {
      this.error = ''

      if (this.userObject.login.length < 5) {
        this.error = ru.userCreate.lengthShouldBeMoreThan5Sym
        return false
      }
      if (!(this.userObject.name.length > 0 )) {
        this.error = ru.userCreate.lengthShouldBeMoreThan0Sym
        return false
      }
      const passShouldBeChecked = (this.isEdit && this.userObject.passwordHash.length > 0) || !this.isEdit
      if ( passShouldBeChecked && !this.isValidPassword(this.userObject.passwordHash)) {
        this.error = ru.userCreate.wrongPassword
        return false
      }
      return true
    },
    mapForEdit(user: IUser) {
      let result = {
        ... deepClone(user)
      }
      delete result.isDeleted
      return result
    }
  },
  computed: {
    isUserAdmin() {
      return this.userStore.isCurrentUserAdmin()
    },
    codeRenderConfig() {
      let result =  {
        value: { style: 'width: 450px', disabled: true }, // для disabled проверяется только наличие поля
        name: { style: 'width: 450px' },
        login: { style: 'width: 450px' },
        passwordHash: { style: 'width: 450px', type: "password" },
        description: { style: 'width: 800px' },
      } as RenderConfig
      return result
    },
  },
  async created() {
    await this.userStore.loadUserList()
    toast.success(ru.codeCreate.userDataLoadedMsg)
    if (this.$route.params.id) { // редактирование, загружаем
      this.isEdit = true
      this.userObject =
        this.mapForEdit(this.userStore.userDictionary[this.$route.params.id as string])

    }
  }

})
</script>

<template>
  <div class="flx flx-item-auto">
    <Sidebar />
    <div class="content flx-item-auto flx-col">
      <section class="section">
        <div class="section__title">
          <span class="section__title-accent">{{ isEdit ? ru.userCreate.captionEdit : ru.userCreate.captionNew }}</span>
        </div>
        <div v-if="error.length > 0" class="settings-error">{{error}}</div>
        <div class="form-container">
          <div v-for="(value, key) in userObject" class="input-group">
            <label>{{ ru.userCreate[key] ?? ''}}</label>
            <input class="inline" :type="codeRenderConfig[key]?.type ?? 'text'" v-model="userObject[key]"
                   :disabled="codeRenderConfig[key]?.hasOwnProperty('disabled') ?? true"
                   :style="codeRenderConfig[key]?.style ?? ''"/>
          </div>
        </div>
        <button class="btn btn_order btn_big" @click="isEdit ? editUser() : createUser()">
          {{ isEdit ? ru.code.update : ru.code.create  }}</button>

      </section>
    </div>
  </div>
</template>

<style lang="scss">
@import "@/style/table.scss";
@import "@/style/form.scss";

.btn {
  margin-right: 20px;
  margin-bottom: 10px;
  margin-top: 10px;
}
.btn_order {
  background: #4caf50;
  color: var(--lynx-white);
}

.settings-error {
  padding: 1rem;
  font-size: 1rem;
  margin-bottom: 1.5rem;
  background: #c0392b;
  color: #ffffff;
  border-radius: .5rem;
}

</style>