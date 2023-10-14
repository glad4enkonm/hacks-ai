import { createRouter, createWebHistory } from "vue-router";
import User from "@/views/User.vue";
import Metrics from "@/views/Metric.vue";
import { useUserStore } from "@/stores/user";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "map-home",
      meta: {authRequired: true},
      component: () => import("../views/Map.vue"),
    },
    {
      path: "/map",
      name: "map",
      meta: {authRequired: true},
      component: () => import("../views/Map.vue"),
    },
    {
      path: "/user",
      name: "user",
      meta: {authRequired: true},
      component: User,
    },
    {
      path: "/metric",
      name: "metric",
      meta: {authRequired: true},
      component: Metrics,
    },
    {
      path: "/user/:id",
      name: "edit user",
      meta: {authRequired: true},
      props: true,
      component: () => import("../views/OneUser.vue"),
    },
    {
      path: "/user/new",
      name: "user new",
      meta: {authRequired: true},
      component: () => import("../views/OneUser.vue"),
    },
    {
      path: "/auth",
      name: "auth",
      meta: {authRequired: false},
      // route level code-splitting
      // this generates a separate chunk ([Name].[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import("../views/Auth.vue"),
    },
  ],
});

router.beforeEach((to, from, next) => {
  try {
    const user = useUserStore();
    const requireAuth = to.matched.some(route => route.meta.authRequired);
    if (requireAuth && (user.token === null || user.token.length == 0)) {
      next('/auth')
    } else {
      next()
    }
  }
  catch (e) {
    next('/auth')
  }

})

export default router;
