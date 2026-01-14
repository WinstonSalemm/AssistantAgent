# ⚡ Быстрая шпаргалка: Переменные для Railway

## 📋 Скопируй и добавь в Railway → Settings → Variables:

```env
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
OpenAI__ApiKey=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
OpenAI__Model=gpt-5-mini
OpenAI__DeepThinkingModel=gpt-5.2
OpenAI__WhisperModel=whisper-1
OpenAI__TTSModel=tts-1
OpenAI__EmbeddingModel=text-embedding-ada-002
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
```

---

## 🎯 Пошагово:

### 1. PostgreSQL:
- Railway Dashboard → "+ New" → Database → PostgreSQL
- Railway автоматически создаст `DATABASE_URL`

### 2. Variables в API сервисе:
- Railway Dashboard → API сервис → Settings → Variables
- "+ New Variable" для каждой строки выше

### 3. Проверка:
- Health: `https://твой-url.railway.app/health`
- Swagger: `https://твой-url.railway.app`

---

**Подробная инструкция:** `docs/RAILWAY_SETUP_GUIDE.md`
