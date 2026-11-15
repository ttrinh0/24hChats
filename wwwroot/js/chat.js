"use strict";

const chat = document.getElementById("chat");
const roomId = chat.dataset.roomId;
const expiresAt = Date.parse(chat.dataset.expiresAt);

const status = document.getElementById("connectionStatus");
const sendButton = document.getElementById("sendButton");
const nicknameInput = document.getElementById("userInput");
const messageInput = document.getElementById("messageInput");
const messages = document.getElementById("messagesList");
const messageBox = document.getElementById("message-box");

let expired = false;
let joined = false;
let sending = false;

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chatHub")
  .withAutomaticReconnect()
  .build();

function updateSendButton() {
  sendButton.disabled = expired || !joined || sending;
}

connection.on("ReceiveMessage", (nickname, message) => {
  if (expired) return;

  const item = document.createElement("li");
  const nametag = document.createElement("span");
  item.className = "mb-2 chatmessage";
  nametag.className = "message-nickname";
  nametag.innerText = `${nickname}`;

  // Treat messages as text, never as HTML.
  item.textContent = `: ${message}`;
  item.prepend(nametag);
  messages.appendChild(item);
  messageBox.scrollTop = messageBox.scrollHeight - messageBox.clientHeight;
});

async function joinRoom() {
  joined = false;
  updateSendButton();

  try {
    await connection.invoke("JoinRoom", roomId);

    if (expired) return;

    joined = true;
    status.textContent = "Connected";
  } catch (error) {
    status.textContent = error.message;
  }

  updateSendButton();
}

connection.onreconnecting(() => {
  joined = false;
  status.textContent = "Connection lost. Reconnecting…";
  updateSendButton();
});

connection.onreconnected(async () => {
  if (!expired) {
    await joinRoom();
  }
});

connection.onclose(() => {
  joined = false;

  if (!expired) {
    status.textContent = "Disconnected. Refresh to reconnect.";
  }

  updateSendButton();
});

async function start() {
  if (expired) return;

  try {
    await connection.start();

    if (expired) {
      await connection.stop();
      return;
    }

    await joinRoom();
  } catch {
    if (!expired) {
      status.textContent = "Unable to connect. Retrying…";
      setTimeout(start, 5000);
    }
  }
}

document
  .getElementById("messageForm")
  .addEventListener("submit", async (event) => {
    event.preventDefault();

    if (!joined || expired || sending) return;

    const nickname = nicknameInput.value.trim();
    const message = messageInput.value.trim();

    if (!nickname || !message) {
      status.textContent = "Enter a nickname and a message.";
      return;
    }

    sending = true;
    updateSendButton();

    try {
      await connection.invoke("SendMessage", nickname, message);

      // Keep any new text typed while the request was in progress.
      if (messageInput.value.trim() === message) {
        messageInput.value = "";
      }

      status.textContent = "Connected";
      messageInput.focus();
    } catch (error) {
      status.textContent = error.message;
    } finally {
      sending = false;
      updateSendButton();
    }
  });

function checkExpiration() {
  if (expired || Date.now() < expiresAt) return;

  expired = true;
  joined = false;

  status.textContent = "This room has expired. Return to all rooms.";
  messageInput.disabled = true;
  updateSendButton();

  void connection.stop();
}

checkExpiration();

if (!expired) {
  void start();
}

const expirationTimer = setInterval(() => {
  checkExpiration();

  if (expired) {
    clearInterval(expirationTimer);
  }
}, 1000);
