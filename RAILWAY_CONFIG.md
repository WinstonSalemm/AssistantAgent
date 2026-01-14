# 🔧 Конфигурация для Railway

## ⚠️ ВАЖНО: Добавь эти переменные в Railway!

В Railway Dashboard → Settings → Variables добавь:

```env
# Database (Railway автоматически создаст)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
ConnectionStrings__Redis=${{Redis.REDIS_URL}}

# OpenAI API Key (КРИТИЧНО!)
OpenAI__ApiKey=sk-your-openai-api-key-here

# OpenAI Models
OpenAI__Model=gpt-5-mini
OpenAI__DeepThinkingModel=gpt-5.2
OpenAI__WhisperModel=whisper-1
OpenAI__TTSModel=tts-1
OpenAI__EmbeddingModel=text-embedding-ada-002

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
```

---

## 🎯 Как работает переключение моделей:

### Базовая модель (gpt-5-mini):
```json
POST /api/chat
{
  "message": "объясни что такое REST API",
  "useDeepThinking": false
}
```
→ Использует **gpt-5-mini** (быстро и дешево)

### Модель "думай глубже" (gpt-5.2):
```json
POST /api/chat
{
  "message": "объясни что такое REST API",
  "useDeepThinking": true
}
```
→ Использует **gpt-5.2** (более глубокий анализ)

---

## 📱 В MAUI App:

Когда пользователь нажимает кнопку "Думай глубже":
```csharp
var request = new ChatRequest
{
    Message = userMessage,
    UseDeepThinking = true  // ← Включает gpt-5.2
};
```

---

**После добавления переменных Railway автоматически перезапустит сервис!**
