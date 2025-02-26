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

const users = new Map(); // Store userId -> socket.id mapping

io.on("connection", (socket) => {
    console.log("✅ A user connected: " + socket.id);

    socket.on("register", (userId) => {
        console.log(`Attempting to register user: ${userId}`);
        
        // Ensure the user is not already registered
        if (users.has(userId)) {
            console.log(`User ${userId} is already online. Updating socket id.`);
        }
        
        // Set or update the user in the Map
        users.set(userId, socket.id);
        console.log(`User ${userId} is now online`);
        console.log("Current users:", Array.from(users.keys())); // Shows all registered users
    });

    socket.on("private_message", ({ sender, receiver, message }) => {
        console.log(`Attempting to send message from ${sender} to ${receiver}`);
    
        if (!users.has(sender)) {
            console.log(`Sender ${sender} is not registered or is offline.`);
            return;
        }
    
        const receiverSocketId = users.get(receiver);
        if (receiverSocketId) {
            const messageData = { sender, message };  // Ensure this is an OBJECT, not an array
            io.to(receiverSocketId).emit("private_message", messageData);
            console.log(`📩 Message sent from ${sender} to ${receiver}: ${message}`);
        } else {
            console.log(`⚠️ User ${receiver} is offline.`);
        }
    });
    

    socket.on("disconnect", () => {
        for (let [userId, socketId] of users.entries()) {
            if (socketId === socket.id) {
                users.delete(userId);
                console.log(`User ${userId} disconnected`);
                break;
            }
        }
    });
});

server.listen(3000, () => {
    console.log("🚀 Server running on http://127.0.0.1:3000");
});
