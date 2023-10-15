<script setup lang="ts">
import Sidebar from "@/components/Sidebar.vue";
import ru from "../util/text";
import { editImageInline } from "@/util/inlineImage";
</script>

<script lang="ts">
import { defineComponent } from "vue";

import { useToast } from 'vue-toastification'
import { useUserStore } from "@/stores/user";
const toast = useToast()

export default defineComponent({
  data() {
    return {
      searchText: "",
      userStore: useUserStore(),
    }
  },
  methods: {
    async deleteUser(userId:any) {
      await this.userStore.deleteUser(userId)
    },
    async editUser(userId:any) {
      this.$router.push({name: "edit user", params: {id: userId}})
    },
    async createUser() {
      this.$router.push({name: "user new"})
    },
  },
  computed: {
    userList() {
      return  (this.searchText != "") ? this.userStore.userList
        .filter(u => u.login.includes(this.searchText) || u.name?.includes(this.searchText)) 
          :this.userStore.userList
    }
  },
  async created() {
    await this.userStore.loadUserList()
    toast.success(ru.user.dataLoadedMsg)
  }
})
</script>

<template>
  <div className="flx flx-item-auto">
    <Sidebar />
    <div className="content flx-item-auto flx-col">
      <section class="section">
        <div className="flx-space-btw">
          <div class="section__title">
            <span class="section__title-accent">{{ ru.metric.caption }}</span>
          </div>
        </div>
        <div className="flx">

          <button class="btn btn_order btn_big" > {{ ru.user.create }}</button>
          <div className="flx-aic">
            <label>{{ "Область" }}</label>
            <div class="input-group">
              <input class="inline" type="text" required v-model="searchText" />
            </div>
          </div>
          <div v-if="false" className="flx-aic">
            <label>{{ ru.map.search }}</label>
            <div class="input-group">
              <input class="inline" type="text" required v-model="searchText" />
            </div>
          </div>
        </div>
        <div className="flx"></div>

        <div class="table">
          <div class="table__row table__header">
            <div class="table__row-item table__row-item-20 flx-jcc">Идентификатор</div>
            <div class="table__row-item table__row-item-30 flx-jcc">Название</div>
            <div class="table__row-item table__row-item-10 flx-jcc">1М</div>
            <div class="table__row-item table__row-item-10 flx-jcc">2М </div>
            <div class="table__row-item table__row-item-10 flx-jcc">3М</div>
            <div class="table__row-item table__row-item-10 flx-jcc">4М</div>
            <div class="table__row-item table__row-item-10 flx-jcc">Итого</div>
            <div class="table__row-item table__row-item-20 flx-jcc">Дата</div>
            <div class="table__row-item table__row-item-20 flx-jcc">Действие</div>
          </div>
          <div v-for="user in []" class="table__row">
            <div class="table__row-item table__row-item-20 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-30 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-10 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-10 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-10 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-10 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-10 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-20 flx-jcc">{{}}</div>
            <div class="table__row-item table__row-item-20 flx-jcc">
              <a href="#" class="icon" :title="ru.image.cartActionDeleteIcon" >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                  <use xlink:href="/delete.svg#Editable-line"></use>
                </svg>
              </a>
              <a href="#" class="icon" :title="ru.image.cartActionEditIcon" >
                <img :src="editImageInline">
              </a>
            </div>
          </div>
        </div>

      </section>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@import "@/style/table.scss";
@import "@/style/form.scss";
@import "@/style/icon.scss";

.btn {
  margin-right: 20px;
  margin-bottom: 10px;
  margin-top: 10px;
}

.btn_order {
  background: #4caf50;
  color: var(--lynx-white);
}

</style>