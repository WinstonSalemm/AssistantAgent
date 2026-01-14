# 🎉 УСПЕХ! API работает на Railway!

## ✅ Что сделано:

- ✅ API задеплоен на Railway
- ✅ Билд прошел успешно
- ✅ Сервис активен и работает

---

## 🔧 Что нужно проверить СЕЙЧАС:

### 1️⃣ Переменные окружения

В Railway Dashboard → Settings → Variables проверь что есть:

```env
# Database (Railway должен был создать автоматически)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}

# Redis (если добавил)
ConnectionStrings__Redis=${{Redis.REDIS_URL}}

# OpenAI (КРИТИЧНО - без этого AI не будет работать!)
OpenAI__ApiKey=sk-твой-реальный-ключ
OpenAI__Model=gpt-4o-mini
OpenAI__WhisperModel=whisper-1
OpenAI__TTSModel=tts-1
OpenAI__EmbeddingModel=text-embedding-ada-002

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
```

⚠️ **ВАЖНО:** Если не добавил OpenAI API key - добавь сейчас! Без него AI агенты не будут работать.

---

### 2️⃣ PostgreSQL Database

Проверь что PostgreSQL сервис создан:

1. В Railway Dashboard должен быть сервис **PostgreSQL**
2. Если нет - создай: "+ New" → Database → PostgreSQL
3. Railway автоматически создаст `DATABASE_URL` переменную
4. Миграции применятся автоматически при старте (мы добавили auto-migrate в Program.cs)

---

### 3️⃣ Тестирование API

#### Health Check:

Открой в браузере:

```
https://твой-url.railway.app/health
```

Должен вернуть:

```json
{
  "status": "Healthy",
  "timestamp": "2026-01-13T..."
}
```

#### Swagger UI:

Открой:

```
https://твой-url.railway.app
```

Должен открыться Swagger UI с документацией API!

#### Тестовый запрос через Swagger:

**POST /api/chat:**

```json
{
  "message": "привет! как дела?",
  "isVoice": false
}
```

Если OpenAI key настроен - получишь ответ от AI!

**POST /api/tasks:**

```json
{
  "title": "Тестовая задача из Railway",
  "priority": 2
}
```

**GET /api/tasks/active** - посмотри список задач

---

### 4️⃣ Проверка логов

В Railway Dashboard → Deploy Logs проверь:

- ✅ Нет ли ошибок при старте
- ✅ Применились ли миграции БД (должно быть: "Database migrations applied successfully")
- ✅ Запустился ли сервер (должно быть: "Starting AI Personal Assistant API")

---

## 🎯 Следующие шаги:

### 1. Настроить MAUI для работы с облачным API

Создай файл `src/Assistant.MAUI/Constants/ApiConfig.cs`:

```csharp
namespace Assistant.MAUI.Constants;

public static class ApiConfig
{
    #if DEBUG
    // Локальная разработка
    public const string BaseUrl = "http://localhost:5000";
    #else
    // Production - Railway
    public const string BaseUrl = "https://твой-url.railway.app";
    #endif

    public const int TimeoutSeconds = 30;
}
```

**Замени `твой-url.railway.app` на реальный URL из Railway!**

### 2. Создать API Service в MAUI

Создай `src/Assistant.MAUI/Services/ApiService.cs`:

```csharp
using Assistant.MAUI.Constants;
using Assistant.Shared.DTOs;
using System.Net.Http.Json;

namespace Assistant.MAUI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            Timeout = TimeSpan.FromSeconds(ApiConfig.TimeoutSeconds)
        };
    }

    public async Task<ChatResponse> SendMessageAsync(string message, bool isVoice = false)
    {
        var request = new ChatRequest
        {
            Message = message,
            IsVoice = isVoice
        };

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ChatResponse>()
            ?? throw new Exception("Failed to parse response");
    }

    public async Task<List<TaskDto>> GetTasksAsync()
    {
        var response = await _httpClient.GetAsync("/api/tasks/active");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TaskDto>>()
            ?? new List<TaskDto>();
    }
}
```

### 3. Тестировать с разных устройств

Теперь можешь:

- ✅ Открыть API с ноута: `https://твой-url.railway.app`
- ✅ Открыть API с телефона (когда MAUI готов)
- ✅ **Одна БД, одна память на всех устройствах!**

---

## 🔥 Что получилось:

✅ **Облачный API** - доступен 24/7 из любой точки мира  
✅ **Облачная БД** - все данные сохраняются в PostgreSQL на Railway  
✅ **Одна память** - AI агент помнит всё на всех устройствах  
✅ **HTTPS** - безопасное соединение  
✅ **Автоматический деплой** - при каждом push в GitHub

---

## 📊 Текущий статус:

| Компонент          | Статус                                 |
| ------------------ | -------------------------------------- |
| Backend API        | ✅ Работает на Railway                 |
| PostgreSQL         | ⚠️ Нужно проверить/создать             |
| OpenAI Integration | ⚠️ Нужно добавить API key              |
| MAUI App           | ⏳ Требует настройки для облачного API |
| Тестирование       | ⏳ В процессе                          |

---

## 🐛 Если что-то не работает:

### API не отвечает:

1. Проверь что сервис активен в Railway Dashboard
2. Проверь Deploy Logs на ошибки
3. Проверь что порт 8080 открыт

### OpenAI не работает:

1. Проверь что `OpenAI__ApiKey` добавлен в Variables
2. Проверь что ключ правильный (начинается с `sk-`)
3. Проверь баланс на https://platform.openai.com/usage

### БД не работает:

1. Проверь что PostgreSQL сервис создан
2. Проверь что `DATABASE_URL` есть в Variables
3. Проверь Deploy Logs - должны быть миграции

---

## 🎉 ПОЗДРАВЛЯЮ!

Твой AI ассистент теперь в облаке!

**Следующий шаг:** Настроить MAUI app для работы с облачным API, и тогда сможешь общаться с AI и на ноуте, и на телефоне с одной памятью! 🚀

---

**URL твоего API:** `https://твой-url.railway.app`  
**Swagger UI:** `https://твой-url.railway.app`  
**Health Check:** `https://твой-url.railway.app/health`
