# AI Personal Assistant

Персональный AI-ассистент с голосовым управлением и агентной архитектурой.

## 🏗️ Архитектура

```
┌─────────────────────────────────────────┐
│         MAUI App (Cross-platform)        │
│   iOS / Android / Windows / macOS        │
└─────────────────┬───────────────────────┘
                  │ HTTPS/REST
                  │
┌─────────────────▼───────────────────────┐
│         ASP.NET Core Web API             │
│  ┌──────────┬──────────┬─────────────┐  │
│  │ AI Agent │ Tasks    │ Memory      │  │
│  │ Router   │ Service  │ Service     │  │
│  └──────────┴──────────┴─────────────┘  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│     PostgreSQL + pgvector + Redis        │
└─────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│           OpenAI API                     │
│   GPT-4 │ Whisper │ TTS                  │
└─────────────────────────────────────────┘
```

## 🎯 Ключевые фичи

### MVP (v1.0)
- ✅ Голосовой ввод/вывод (STT/TTS)
- ✅ AI агентная архитектура
- ✅ Управление задачами
- ✅ Система напоминаний
- ✅ Контекстная память
- ✅ Быстрый доступ (hotkeys, widgets)
- ✅ Кроссплатформенность

### Технологический стек

**Frontend:**
- .NET MAUI 8.0 (iOS, Android, Windows, macOS)
- CommunityToolkit.MAUI
- Plugin.Maui.Audio (голосовой ввод/вывод)

**Backend:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- SignalR (real-time communication)
- Hangfire (background jobs)

**Database:**
- PostgreSQL 16
- Redis (кэширование)
- pgvector (векторный поиск для памяти)

**AI:**
- OpenAI API (GPT-4, Whisper, TTS)
- Semantic Kernel (AI оркестрация)
- LangChain.NET (агентная архитектура)

**Infrastructure:**
- Railway (hosting)
- Docker
- GitHub Actions (CI/CD)

## 📁 Структура проекта

```
src/
├── Assistant.API/              # Web API
│   ├── Controllers/           # API endpoints
│   ├── Hubs/                  # SignalR hubs
│   ├── Middleware/            # Custom middleware
│   └── Program.cs
│
├── Assistant.Core/            # Domain layer
│   ├── Entities/             # Domain entities
│   ├── Interfaces/           # Abstractions
│   ├── Services/             # Business logic
│   └── Enums/
│
├── Assistant.Infrastructure/  # Infrastructure layer
│   ├── Data/                 # EF Core, DbContext
│   ├── AI/                   # OpenAI integration
│   ├── Agents/               # AI agents
│   ├── Services/             # External services
│   └── Repositories/
│
├── Assistant.MAUI/           # Mobile/Desktop app
│   ├── Views/               # UI pages
│   ├── ViewModels/          # MVVM ViewModels
│   ├── Services/            # Platform services
│   ├── Platforms/           # Platform-specific code
│   └── MauiProgram.cs
│
└── Assistant.Shared/         # Shared code
    ├── DTOs/                # Data Transfer Objects
    ├── Contracts/           # API contracts
    └── Constants/
```

## 🚀 Быстрый старт

### Требования
- .NET 8.0 SDK или выше
- Visual Studio 2022 / VS Code
- Docker Desktop (для PostgreSQL + Redis)
- OpenAI API Key

### 1. Запусти базы данных через Docker:

```bash
docker-compose up -d
```

Это запустит PostgreSQL (с pgvector) и Redis.

### 2. Настрой OpenAI API Key:

Отредактируй `src/Assistant.API/appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-YOUR-REAL-API-KEY-HERE"
  }
}
```

### 3. Примени миграции:

```bash
cd src/Assistant.API
dotnet ef database update --project ../Assistant.Infrastructure/Assistant.Infrastructure.csproj
```

### 4. Запусти Backend:

```bash
dotnet run
```

Или просто нажми **F5** в Visual Studio!

Backend запустится на: http://localhost:5000

Swagger UI откроется автоматически!

### 5. Протестируй API:

Открой Swagger UI → **POST /api/chat** → Try it out:

```json
{
  "message": "привет! добавь задачу купить молоко",
  "isVoice": false
}
```

**Готово! 🎉** Ассистент работает!

📖 **Подробные инструкции:** [docs/QUICKSTART.md](docs/QUICKSTART.md)

## 🔑 Environment Variables

Создай `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=assistant;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  },
  "OpenAI": {
    "ApiKey": "your_openai_api_key",
    "Model": "gpt-4",
    "WhisperModel": "whisper-1",
    "TTSModel": "tts-1"
  }
}
```

## 📱 Быстрый доступ

**Windows:**
- Global Hotkey: `Ctrl+Shift+Space`
- System Tray icon

**Android:**
- Quick Settings Tile
- Home Screen Widget
- Volume button long press

**iOS:**
- Lock Screen Widget
- Shortcuts integration
- Control Center

**macOS:**
- Global Hotkey: `Cmd+Shift+Space`
- Menu Bar app

## 🤖 AI Агенты

### Command Router Agent
Определяет intent пользовательского запроса и направляет к нужному агенту.

### Task Agent
Управление задачами: создание, редактирование, удаление, поиск.

### Reminder Agent
Система напоминаний с поддержкой natural language времени.

### Query Agent
Ответы на вопросы, объяснения, поиск информации.

### Memory Agent
Управление контекстом и долгосрочной памятью пользователя.

## 📊 Database Schema

```sql
-- Messages (история чата)
messages: id, role, content, timestamp, session_id

-- Tasks (задачи)
tasks: id, title, description, completed, priority, due_date, created_at

-- Reminders (напоминания)
reminders: id, title, remind_at, completed, recurring, created_at

-- Memory (долгосрочная память)
memory: id, content, embedding (vector), metadata, created_at

-- User Preferences
preferences: id, key, value, created_at
```

## 🎨 UI/UX

Минималистичный дизайн с фокусом на скорость:
- Темная тема по умолчанию
- Голосовая кнопка всегда доступна
- Минимум кликов для любого действия
- Анимации только где нужно

## 🔄 Development Workflow

1. Backend API разработка
2. Тестирование через Swagger
3. MAUI UI implementation
4. End-to-end тестирование
5. Deploy на Railway

## 📝 Roadmap

### Phase 1 (Текущая) - MVP
- [x] Архитектура проекта
- [ ] Backend API setup
- [ ] Database schema
- [ ] OpenAI integration
- [ ] MAUI app setup
- [ ] Базовый UI
- [ ] Voice input/output

### Phase 2 - AI Agents
- [ ] Command Router Agent
- [ ] Task Agent
- [ ] Reminder Agent
- [ ] Query Agent
- [ ] Memory Agent

### Phase 3 - Advanced Features
- [ ] Векторная память (pgvector)
- [ ] Offline режим
- [ ] Кастомные workflows
- [ ] Голосовая активация
- [ ] Умные напоминания

### Phase 4 - Polish
- [ ] Performance optimization
- [ ] UI/UX improvements
- [ ] Platform-specific features
- [ ] Documentation

## 📄 License

Personal use only.
#   A s s i s t a n t A g e n t  
 