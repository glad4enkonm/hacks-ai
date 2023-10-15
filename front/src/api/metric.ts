import api from "@/api";
import type { IMetric } from "@/model/metric";

class MetricApiService {
  static getInstance(): MetricApiService {
    return this.instance || (this.instance = new this())
  }

  private static instance: MetricApiService

  private baseUrl = '/metric'

  private constructor() {
    // empty for linter
  }

  getProxy(query: string) {
    return api.get<string>(this.baseUrl + '/proxy/' + query)
  }

  // Операции для Metric -------------------------------
  // Запросить данные
  getMetric(id: bigint) {
      return api.get<IMetric>(this.baseUrl + '/metric' + '/' + id)
  }

  getMetricList() {
      return api.get<IMetric[]>(this.baseUrl + '/metric' + '/list')
  }

  // Создать данные
  createMetric(payload: IMetric) {
      return api.post<IMetric>(this.baseUrl + '/metric', payload)
  }

  // Удалить данные
  deleteMetric(id: bigint) {
      return api.delete(this.baseUrl + '/metric' + '/' + id)
  }

}

export default MetricApiService.getInstance()
