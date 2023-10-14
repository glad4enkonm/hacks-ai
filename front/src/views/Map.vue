<script setup lang="ts">
import Sidebar from "@/components/Sidebar.vue";
import { doneImageInline, editImageInline , sendImageInline} from "@/util/inlineImage";
</script>

<script lang="ts">
import { useToast } from "vue-toastification";

const toast = useToast()

import { defineComponent } from "vue";

import { useUserStore } from "@/stores/user";
import ru from "@/util/text";

import { deepClone } from "@/util/diff";
export default defineComponent({
  data() {
    return {
      searchText: "",
      userStore: useUserStore(),
    };
  },
  methods: {
    async loadData() {      
      await this.userStore.getUser()

      toast.success(ru.map.loadedMessage)
    },
    copyToClipboard(value: any) {
      navigator.clipboard.writeText(value)
    },
    async deleteCode(codeId:any) {
      
    },
    async editCode(codeId:any) {
      this.$router.push({name: "edit code", params: {id: codeId}})
    },
    
    
    async createCode() {
      this.$router.push({name: "code new"})
    },
    formatValue(code: any) {      
      if (this.isAdmin)
        return  code.value
      else if (code.templateId != null)
        return ru.map.digital
      else
        return ru.map.printed

    },
  },
  computed: {

    isAdmin() {
      return this.userStore.isCurrentUserAdmin()
    },
    lastCreatedCodeId() {

    }
  },
  created() {
    this.loadData()
  },
});
</script>

<template>
  <div className="flx flx-item-auto">
    <Sidebar />
    <div className="content flx-item-auto flx-col">
      <section class="section">
        <div className="flx-space-btw">
          <div class="section__title">
            <span class="section__title-accent">{{ ru.map.caption }}</span>
          </div>
        </div>
        <div className="flx">

          <button class="btn btn_order btn_big" @click="createCode()"> {{ ru.map.create }}</button>
          <div className="flx-aic">
            <label>{{ ru.map.search }}</label>
            <div class="input-group">
              <input class="inline" type="text" required v-model="searchText" />
            </div>
          </div>
        </div>
        <iframe id="f1" ref="frame1" :src="'/map.html'"></iframe>


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

.last-created {
  font-weight: bold;
  color: #4caf50;
}

#summary-grid {
  height: 90px;
}

#items-grid-div {
  min-height: 100px;
  overflow: hidden;
}
#items-grid {
}

.photo-url {
  height: 32px;
}

iframe {
    width: 100%;
    height: 100%;
    border: none;
    width: 1000px;
    height: 800px;
}

</style>
