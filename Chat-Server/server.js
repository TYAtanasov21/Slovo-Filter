const express = require("express");
const http = require("http");
const { Server } = require("socket.io");

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

io.on("connection", (socket) => {
    console.log("✅ A user connected: " + socket.id);

    socket.on("send_message", (message) => {
        console.log("📩 Received message: " + message);
        io.emit("receive_message", message);
    });

    socket.on("disconnect", () => {
        console.log("❌ User disconnected: " + socket.id);
    });
});

server.listen(3000, () => {
    console.log("🚀 Server running on http://127.0.0.1:3000");
});
