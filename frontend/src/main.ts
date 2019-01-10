import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store/store";
import VModal from "vue-js-modal";

Vue.use(VModal);

Vue.config.productionTip = false;

new Vue({
  router,
  render: h => h(App),
  store
}).$mount("#app");
