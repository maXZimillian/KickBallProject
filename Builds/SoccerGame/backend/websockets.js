const fs = require("fs");
const WebSocket = require("ws");
const { v4: uuidv4 } = require("uuid"); // Для генерации уникальных ID

const options = {
    cert: fs.readFileSync("/etc/letsencrypt/live/tma-game.ru/fullchain.pem"),
    key: fs.readFileSync("/etc/letsencrypt/live/tma-game.ru/privkey.pem")
};

const wss = new WebSocket.Server({ port: 3000 });

let rooms = {}; // Хранение лобби и их состояния

wss.on("connection", (ws) => {
    console.log("Новое подключение к WebSocket");
    
    let playerID = uuidv4();
    let roomID = null;

    ws.on("message", (message) => {
        let data = JSON.parse(message);
        console.log("Пришло сообщение:", data);

        if (data.type === "join") {
            roomID = findOrCreateRoom(ws, playerID);
        }

        if (roomID && rooms[roomID]) {
            if (data.type === "ball_stats" || data.type === "gk_stats" || data.type === "gk_animation") {
                rooms[roomID].players.forEach(client => {
                    if (client.ws !== ws) {
                        client.ws.send(JSON.stringify(data));
                    }
                });
            }
            else if (data.type === "kick_start" || data.type === "kick_late" || data.type === "goal" || data.type === "restart" ) {
                rooms[roomID].players.forEach(client => {
                    client.ws.send(JSON.stringify(data));
                });
            }
        }
    });

    ws.on("close", () => {
        console.log(`Игрок ${playerID} отключился`);
        if (roomID && rooms[roomID]) {
            rooms[roomID].players = rooms[roomID].players.filter(client => client.ws !== ws);
            if (rooms[roomID].players.length === 0) {
                console.log(`Удаляем комнату ${roomID}`);
                delete rooms[roomID];
            } else {
                console.log(`Игрок вышел из комнаты ${roomID}`);
                rooms[roomID].players[0].ws.send(JSON.stringify({ type: "player_left" }));
            }
        }
    });
});

function findOrCreateRoom(ws, playerID) {
    let roomID = Object.keys(rooms).find(id => rooms[id].players.length === 1);
    if (!roomID) {
        roomID = uuidv4();
        rooms[roomID] = { players: [], ballState: "Default" };
    }
    
    rooms[roomID].players.push({ ws, id: playerID });
    console.log(`Игрок ${playerID} подключился к комнате ${roomID}`);
    
    if (rooms[roomID].players.length === 2) {
        let roles = Math.random() > 0.5 ? ["kicker", "goalkeeper"] : ["kicker", "goalkeeper"];
        rooms[roomID].players.forEach((client, index) => {
            client.ws.send(JSON.stringify({ type: "start", role: roles[index], playerID: client.id }));
        });
        console.log(`Заполнена комната ${roomID}`);
    }
    return roomID;
}

console.log("WebSocket сервер запущен на порту 3000");