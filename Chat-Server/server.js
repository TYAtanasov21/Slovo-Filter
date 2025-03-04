const express = require("express");
const http = require("http");
const { Server } = require("socket.io");
const axios = require('axios');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const server = http.createServer(app);

const io = new Server(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

const users = new Map(); // Store userId -> socket.id mapping

// Define the API base URL
const API_BASE_URL = "https://slovo-filter-back-end-a8abcea3a0hcftge.northeurope-01.azurewebsites.net/api";

// When a user connects
io.on("connection", (socket) => {
    console.log("User connected: " + socket.id);

    // Register user
    socket.on("register", async (userId) => {
        console.log(`Attempting to register user: ${userId}`);

        if (users.has(userId)) {
            console.log(`User ${userId} is already online. Updating socket id.`);
        }

        // Set or update the user in the Map
        users.set(userId, socket.id);
        console.log(`User ${userId} is now online`);
        console.log("Current users:", Array.from(users.keys())); // Shows all registered users

        // Fetch offline messages from the C# API
        try {
            const response = await axios.get(`${API_BASE_URL}/messages/unread/${userId}`);
            const offlineMessages = response.data;

            if (offlineMessages && offlineMessages.length > 0) {
                offlineMessages.forEach(async (msg) => {
                    socket.emit("private_message", {
                        sender: msg.senderId,
                        receiver: msg.receiverId,
                        message: msg.content,
                        date: msg.date
                    });
                    
                    // Mark message as delivered
                    try {
                        await axios.put(`${API_BASE_URL}/messages/${msg.id}/deliver`);
                        console.log(`Message ${msg.id} marked as delivered`);
                    } catch (markError) {
                        console.error('Error marking message as delivered:', markError);
                    }
                });
                console.log('Offline messages sent to the user:', offlineMessages);
            }
        } catch (error) {
            console.error('Error fetching offline messages:', error.message);
        }
    });

    // Handle request for message history
    socket.on("get_history", async ({ user1Id, user2Id, limit }) => {
        try {
            console.log("getting history");
            const response = await axios.get(`${API_BASE_URL}/messages/history?user1Id=${user1Id}&user2Id=${user2Id}&limit=${limit || 50}`);
            socket.emit("message_history", response.data);
        } catch (error) {
            console.error('Error fetching message history:', error.message);
            socket.emit("error", { message: "Failed to fetch message history" });
        }
    });

    // Handle private message sending
    socket.on("private_message", async ({ sender, receiver, message }) => {
        console.log(`Attempting to send message from ${sender} to ${receiver}`);

        if (!users.has(sender)) {
            console.log(`Sender ${sender} is not registered or is offline.`);
            return;
        }

        const receiverSocketId = users.get(receiver);
        
        // Store the message in the database regardless of user's online status
        try {
            const response = await axios.post(`${API_BASE_URL}/messages`, {
                senderId: sender,
                receiverId: receiver,
                content: message
            });
            
            const messageId = response.data.messageId;
            console.log(`Message stored in database with ID: ${messageId}`);
            
            // If receiver is online, send the message and mark as delivered
            if (receiverSocketId) {
                const messageData = { 
                    sender, 
                    receiver, 
                    message,
                    date: new Date().toISOString(),
                    id: messageId
                };
                io.to(receiverSocketId).emit("private_message", messageData);
                console.log(`📩 Message sent from ${sender} to ${receiver}: ${message}`);
                
                // Mark as delivered since receiver is online
                try {
                    await axios.put(`${API_BASE_URL}/messages/${messageId}/deliver`);
                } catch (markError) {
                    console.error('Error marking message as delivered:', markError);
                }
            } else {
                console.log(`⚠️ User ${receiver} is offline. Message stored for later delivery.`);
            }
        } catch (error) {
            console.error('Error saving message:', error.message);
            socket.emit("error", { message: "Failed to send message" });
        }
    });

    // Disconnect handler
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

// Express route to check server status
app.get('/status', (req, res) => {
    res.json({ status: 'online', users: Array.from(users.keys()) });
});

server.listen(process.env.PORT || 3000, () => {
    console.log("🚀 Server running on http://127.0.0.1:3000");
});