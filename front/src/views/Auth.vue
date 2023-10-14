<script lang="ts">
import { useUserStore } from "@/stores/user";
import type { IUser } from "@/model/user";
import { defineComponent } from "vue";
import { hash } from "@/util/hash";
import { useErrorStore } from "@/stores/error";
import ru from "../util/text";

const userStore = useUserStore()

export default defineComponent({
  data() {
    return {
      ru: ru,
      error: '',
      login: "",
      password: "",
      errorStore: useErrorStore()
    }
  },
  methods: {
    handleSubmit: async function() {
      const user: IUser = {
        login: this.login,
        passwordHash: await hash(this.password)
      };
      await userStore.auth(user);
      this.error = this.errorStore.apiError;
      this.$router.push({ name: "map" });
    },
    handleLoginChange() {
      this.errorStore.apiError = ''
    },
    handlePasswordChange() {
      this.errorStore.apiError = ''
    }
  }
})
</script>

<template>
  <div class="flx-aic-jcc flx-item-auto login-bg">
    <div class="login-card">
      <div class="login-card__title">{{ ru.login.caption }}</div>
      <div v-if="error.length > 0" class="login-card__error">{{error}}</div>
      <form @submit.prevent="handleSubmit">
        <div class="input-group">
          <label>{{ ru.login.login }}</label>
          <input
            type="text"
            id="login"
            autoComplete="off"
            required
            v-model="login"
            @onChange="handleLoginChange"
          />
        </div>
        <div class="input-group">
          <label >{{ ru.login.password }}</label>
          <input
            type="password"
            id="password"
            autoComplete="off"
            required
            v-model="password"
            @onChange="handlePasswordChange"
          />
        </div>
        <button type="submit" class="btn btn_primary btn_big btn_block">{{ ru.login.signIn }}</button>
      </form>
    </div>
  </div>
</template>

<style lang="scss">

@import "@/style/form.scss";

.login-bg {
  background: var(--naval);
}

.login-card {
  background: var(--gray);
  padding: 2rem;
  box-shadow: 0 .5rem 2rem rgba(#000000, .2);
  border-radius: 1rem;
  width: 20rem;

&__title {
   font-size: 1.5rem;
   font-weight: 600;
   text-align: center;
   margin-bottom: 1.5rem;
 }

&__error {
   padding: 1rem;
   font-size: .75rem;
   margin-bottom: 1.5rem;
   background: #c0392b;
   color: #ffffff;
   border-radius: .5rem;
 }
}

</style>
