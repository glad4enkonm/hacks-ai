<script lang="ts">

import { useUserStore } from "@/stores/user";
import { defineComponent } from "vue";
import ru from "../util/text";

interface NavLink {
  to: string;
  key: string;
}

export default defineComponent({
  data() {
    return {
      userStore: useUserStore(),
    }
  },
  computed: {
    ru() {
      return ru
    },
    navLinks(): NavLink[] {
      const isAdmin = this.userStore.isCurrentUserAdmin()
      return isAdmin ? [
        {
          to: '/map',
          key: ru.map.caption,
        },
        {
          to: '/user',
          key: ru.user.caption,
        }
      ]
    :
      [
        {
          to: '/map',
          key: ru.map.caption,
        }
      ];
    }
  },
  methods: {
    logout() {
      useUserStore().logout()
    }
  },
  async created() {
    await this.userStore.getUser()
  }
})
</script>
<template>
  <nav class="sidebar">
    <div class="flx-item-auto">
      <router-link v-for="link in navLinks" :to="link.to" class="sidebar__item">
      {{ link.key }}
      </router-link>
    </div>
    <div class="sidebar__item" @click="logout">{{ ru.login.signOut }}</div>
  </nav>
</template>

<style lang="scss">
@import "@/style/sidebar.scss";
</style>