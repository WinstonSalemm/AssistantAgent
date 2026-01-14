# 🚀 Quick Start Guide

## Предварительные требования

- **.NET 8.0 SDK** или выше
- **PostgreSQL 16+** с расширением pgvector
- **Redis** (опционально, для кэширования)
- **OpenAI API Key**
- **Visual Studio 2022** или **VS Code**

---

## 1️⃣ Setup PostgreSQL с pgvector

### Windows (через Docker - рекомендуется):

```powershell
# Скачай и установи Docker Desktop
# Затем запусти PostgreSQL с pgvector:
docker run -d \
  --name assistant-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=assistant \
  -p 5432:5432 \
  pgvector/pgvector:pg16

# Проверь что работает:
docker ps
```

### Альтернатива - Установка на Windows:

1. Скачай PostgreSQL 16: https://www.postgresql.org/download/windows/
2. Установи pgvector расширение:
   - Скачай: https://github.com/pgvector/pgvector/releases
   - Следуй инструкциям для Windows

---

## 2️⃣ Setup Redis (опционально)

### Через Docker:

```powershell
docker run -d --name assistant-redis -p 6379:6379 redis:latest
```

### Альтернатива:

Скачай Redis для Windows: https://github.com/microsoftarchive/redis/releases

---

## 3️⃣ Настройка Backend

### Клонируй проект и открой в Visual Studio:

```powershell
cd C:\Users\Legion\Desktop\AssistantProject
code .  # или открой в Visual Studio
```

### Настрой `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=assistant;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "OpenAI": {
    "ApiKey": "sk-YOUR-OPENAI-API-KEY",
    "Model": "gpt-4o-mini",
    "WhisperModel": "whisper-1",
    "TTSModel": "tts-1",
    "EmbeddingModel": "text-embedding-ada-002"
  }
}
```

⚠️ **Важно:** Замени `sk-YOUR-OPENAI-API-KEY` на свой реальный API ключ от OpenAI!

### Примени миграции БД:

```powershell
cd src/Assistant.API

# Создай БД и таблицы:
dotnet ef database update --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj

# Если миграций нет, создай:
dotnet ef migrations add InitialCreate --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
dotnet ef database update --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
```

### Запусти Backend:

```powershell
dotnet run --project src/Assistant.API/Assistant.API.csproj
```

Или нажми **F5** в Visual Studio.

Backend запустится на: **https://localhost:5001** или **http://localhost:5000**

Swagger UI: **https://localhost:5001** (откроется автоматически)

---

## 4️⃣ Проверь что работает

### 1. Health Check:

Открой в браузере: http://localhost:5000/health

Должен вернуть:
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-13T..."
}
```

### 2. Swagger UI:

Открой: http://localhost:5000

Увидишь все доступные API endpoints.

### 3. Тестовый запрос через Swagger:

1. Открой **POST /api/chat**
2. Нажми "Try it out"
3. Введи:
```json
{
  "message": "привет, как дела?",
  "isVoice": false
}
```
4. Нажми "Execute"

Должен вернуть ответ от AI ассистента!

### 4. Создай задачу:

**POST /api/tasks:**
```json
{
  "title": "Купить молоко",
  "priority": 2,
  "dueDate": "2026-01-15T10:00:00Z"
}
```

**GET /api/tasks/active** - посмотри активные задачи

---

## 5️⃣ Тестовые команды для ассистента

Попробуй эти команды через `/api/chat`:

### Управление задачами:
```
"добавь задачу купить молоко"
"покажи список задач"
"что у меня на сегодня?"
"отметь задачу как выполненную"
```

### Напоминания:
```
"напомни мне через час про встречу"
"покажи активные напоминания"
```

### Общие вопросы:
```
"объясни что такое REST API"
"как работает Docker?"
"дай совет по продуктивности"
```

---

## 6️⃣ Структура проекта

```
src/
├── Assistant.API/              # Web API (порт 5000/5001)
├── Assistant.Core/             # Domain models
├── Assistant.Infrastructure/   # Database, AI, Agents
├── Assistant.Shared/          # DTOs
└── Assistant.MAUI/            # Mobile/Desktop app (в разработке)
```

---

## 🔧 Полезные команды

### Пересоздать БД:

```powershell
cd src/Assistant.API
dotnet ef database drop --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj --force
dotnet ef database update --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
```

### Посмотреть логи:

Логи сохраняются в: `src/Assistant.API/logs/`

### Остановить Docker контейнеры:

```powershell
docker stop assistant-postgres assistant-redis
docker rm assistant-postgres assistant-redis
```

---

## 🐛 Troubleshooting

### PostgreSQL не подключается:

1. Проверь что Docker контейнер запущен: `docker ps`
2. Проверь connection string в `appsettings.Development.json`
3. Попробуй подключиться вручную:
   ```powershell
   docker exec -it assistant-postgres psql -U postgres -d assistant
   ```

### OpenAI API ошибки:

1. Проверь API key в `appsettings.Development.json`
2. Проверь баланс на https://platform.openai.com/usage
3. Проверь лимиты rate limit

### Миграции не применяются:

```powershell
# Удали старую БД и создай заново
dotnet ef database drop --force --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
dotnet ef database update --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
```

---

## 📚 Что дальше?

1. **Настрой MAUI app** для мобильных устройств
2. **Добавь новых агентов** (ReminderAgent, MemoryAgent)
3. **Настрой deployment на Railway**
4. **Добавь push уведомления**

---

## 🎯 API Endpoints

### Chat:
- `POST /api/chat` - Отправить сообщение
- `POST /api/chat/voice` - Отправить голосовое сообщение
- `GET /api/chat/history` - История сообщений

### Tasks:
- `GET /api/tasks` - Все задачи
- `GET /api/tasks/active` - Активные задачи
- `GET /api/tasks/completed` - Завершенные задачи
- `POST /api/tasks` - Создать задачу
- `PUT /api/tasks/{id}` - Обновить задачу
- `DELETE /api/tasks/{id}` - Удалить задачу

### Reminders:
- `GET /api/reminders` - Все напоминания
- `GET /api/reminders/active` - Активные напоминания
- `POST /api/reminders` - Создать напоминание
- `PUT /api/reminders/{id}` - Обновить напоминание
- `DELETE /api/reminders/{id}` - Удалить напоминание

---

**Готово! 🎉 Твой AI ассистент запущен и готов к работе!**
