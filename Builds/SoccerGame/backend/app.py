from fastapi import FastAPI, Request
from fastapi.staticfiles import StaticFiles
from aiogram import Bot, Dispatcher, types
import logging
import os
from aiogram.types import Update, InlineKeyboardMarkup, InlineKeyboardButton
from aiogram.utils.executor import start_webhook
from fastapi.responses import FileResponse

# Инициализация логирования
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Инициализация бота
API_TOKEN = "8164101649:AAFGy5qYM8jsseUKKXe_cW3S5qjmjMznrV8"
bot = Bot(token=API_TOKEN)
dp = Dispatcher(bot)

# Устанавливаем бота в контекст
Bot.set_current(bot)

# Создаем объект FastAPI
app = FastAPI()

# Указываем директорию для статики
app.mount("/static", StaticFiles(directory="/var/www/SoccerGame/static"), name="static")

@app.get("/index.html")
async def serve_index():
    return FileResponse("/var/www/SoccerGame/index.html")

@app.get("/manifest.webmanifest")
async def serve_index():
    return FileResponse("/var/www/SoccerGame/manifest.webmanifest")

@app.get("/ServiceWorker.js")
async def serve_index():
    return FileResponse("/var/www/SoccerGame/ServiceWorker.js")

app.mount("/Build", StaticFiles(directory="/var/www/SoccerGame/Build"), name="build")
app.mount("/TemplateData", StaticFiles(directory="/var/www/SoccerGame/TemplateData"), name="templatedata")

# Эндпоинт для получения и проверки информации о вебхуке
@app.get("/webhook")
async def get_webhook_info():
    try:
        webhook_info = await bot.get_webhook_info()
        return webhook_info
    except Exception as e:
        return {"error": str(e)}

# Вебхук для получения обновлений от Telegram
@app.post("/webhook")
async def webhook(request: Request):
    try:
        update = await request.json()
        update = Update(**update)

        await dp.process_update(update)
        return {"status": "ok"}
    except Exception as e:
        logger.error(f"Ошибка при обработке вебхука: {e}")
        return {"error": str(e)}

# Обработчик команды /start
@dp.message_handler(commands=['start'])
async def send_welcome(message: types.Message):
    keyboard = InlineKeyboardMarkup().add(
        InlineKeyboardButton(
            text="🚀 Запустить игру",
            web_app=types.WebAppInfo(url="https://tma-game.ru/index.html")
        )
    )
    await message.reply("Добро пожаловать! Нажмите кнопку ниже, чтобы запустить игру:", reply_markup=keyboard)

# Запускаем бота
@app.on_event("startup")
async def on_startup():
    try:
        webhook_url = "https://tma-game.ru/webhook"
        await bot.set_webhook(webhook_url)
        logger.info(f"Webhook установлен на {webhook_url}")
    except Exception as e:
        logger.error(f"Ошибка при установке вебхука: {e}")

# Главная страница
@app.get("/")
def read_root():
    return {"message": "Telegram Mini App Backend is Running"}
