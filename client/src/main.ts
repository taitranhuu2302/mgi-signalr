import "./assets/main.css";

import { createApp } from "vue";
import { createPinia } from "pinia";

import App from "./App.vue";
import router from "./router";
import signalr from './plugins/signalr'

const app = createApp(App);

app.use(signalr)
app.use(createPinia());
app.use(router);

app.mount("#app");
