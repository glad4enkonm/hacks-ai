import { defineStore } from "pinia";
import type { IMetric } from "@/model/metric";
import metricService from "@/api/metric"
import type { IEntityDictionary } from "@/util/dictionary";
import {
  createElementBase,
  deleteElementBase,
  loadElementBase,
  loadElementListBase,
  updateElementBase
} from "@/util/store";

export const metricStore = defineStore({
  id: "metric",
  state: () => ({
    metricDictionary: {} as IEntityDictionary<IMetric>,
    metricList: [] as IMetric[],
  }),
  actions: {
    // Операции для Metric -------------------------------
    // Запросить данные
    async loadMetricList() {
      await loadElementListBase(() => metricService.getMetricList(),
        entity => entity.metricId,
        (newDictionary, originalList) => {
          this.metricDictionary = newDictionary
          this.metricList = originalList
        })
    },

    async loadMetric(id: bigint) {
      const that = this
      return await loadElementBase(id, id =>  metricService.getMetric(id),
        entity => entity.metricId,
        function(id, entity) {
            that.metricDictionary[id] = entity
            that.metricList = that.metricList.filter(e =>
                e.metricId?.toString() != id)
        })
    },
    // Создать данные
    async createMetric(payload:  IMetric) {
      const that = this
      return await createElementBase(payload, p =>  metricService.createMetric(p),
        entity => entity.metricId,
        function(id, entity) {
            that.metricDictionary[id] = entity
            that.metricList.push(entity)
        })
    },
    // Удалить данные
    async deleteMetric(id: bigint) {
      const that = this
      return await deleteElementBase<IMetric>(id,
        id => metricService.deleteMetric(id),
        id => this.metricDictionary[id],
        function(id) {
            delete that.metricDictionary[id]
            that.metricList = that.metricList.filter(e => e.metricId?.toString() != id)
        })
    },
    async load() {

    },
  },
});
