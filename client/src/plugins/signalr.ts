import { HubConnectionBuilder } from "@microsoft/signalr";
import mitt from "mitt";

export default {
  install: async (app) => {
    const connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5216/chat")
      .build();

    const chatHub = mitt();
    
    function start() {
      return connection.start().catch((err) => {
        console.error("Failed to connect with hub", err);
        return new Promise((resolve, reject) =>
        setTimeout(() => start().then(resolve).catch(reject), 5000)
        );
      });
    }
    connection.onclose(() => start());
    
    connection.on("ReceiveMessage", (user, message) => {
      chatHub.emit("ReceiveMessage", {user, message})
    })    
    
    start();

    chatHub.on("SendMessage", (data) => {
      connection.invoke("SendMessage", data)
    })
    chatHub.on('JoinRoom', (data) => {
      connection.invoke("JoinRoom", data)
    })
    
    app.provide("chatHub", chatHub);
  },
};
