# Архитектура AI Personal Assistant

## 🎯 Общая архитектура

### Layered Architecture (Clean Architecture)

```
┌────────────────────────────────────────────────┐
│              Presentation Layer                │
│         (Assistant.MAUI - UI/UX)               │
│  Views │ ViewModels │ Platform Services        │
└────────────────────┬───────────────────────────┘
                     │ HTTP/SignalR
┌────────────────────▼───────────────────────────┐
│              Application Layer                 │
│            (Assistant.API)                     │
│  Controllers │ Hubs │ Middleware               │
└────────────────────┬───────────────────────────┘
                     │
┌────────────────────▼───────────────────────────┐
│              Business Layer                    │
│            (Assistant.Core)                    │
│  Domain Models │ Services │ Interfaces         │
└────────────────────┬───────────────────────────┘
                     │
┌────────────────────▼───────────────────────────┐
│            Infrastructure Layer                │
│         (Assistant.Infrastructure)             │
│  Data Access │ AI Services │ External APIs     │
└────────────────────┬───────────────────────────┘
                     │
┌────────────────────▼───────────────────────────┐
│         Database & External Services           │
│  PostgreSQL │ Redis │ OpenAI │ Railway         │
└────────────────────────────────────────────────┘
```

---

## 🧩 Компоненты системы

### 1. **Assistant.MAUI** (Presentation)

**Назначение:** Кроссплатформенное приложение для iOS, Android, Windows, macOS.

**Структура:**
```
Assistant.MAUI/
├── Views/                    # UI страницы
│   ├── MainPage.xaml        # Главный экран
│   ├── ChatPage.xaml        # Чат с ассистентом
│   ├── TasksPage.xaml       # Список задач
│   └── SettingsPage.xaml    # Настройки
│
├── ViewModels/              # MVVM ViewModels
│   ├── MainViewModel.cs
│   ├── ChatViewModel.cs
│   ├── TasksViewModel.cs
│   └── BaseViewModel.cs
│
├── Services/                # Platform services
│   ├── IApiService.cs
│   ├── ApiService.cs
│   ├── IAudioService.cs
│   ├── AudioService.cs
│   ├── IStorageService.cs
│   └── StorageService.cs
│
├── Platforms/               # Platform-specific code
│   ├── Android/
│   │   ├── MainActivity.cs
│   │   └── VoiceService.cs
│   ├── iOS/
│   │   ├── AppDelegate.cs
│   │   └── VoiceService.cs
│   └── Windows/
│       ├── App.xaml.cs
│       └── VoiceService.cs
│
├── Models/                  # UI models
├── Converters/             # Value converters
├── Resources/              # Images, fonts, styles
└── MauiProgram.cs          # App startup
```

**Ключевые фичи:**
- MVVM pattern с CommunityToolkit.Mvvm
- Dependency Injection
- Voice recording/playback
- Local caching (SQLite)
- Background services
- Platform-specific integrations

---

### 2. **Assistant.API** (Application Layer)

**Назначение:** REST API + SignalR для real-time communication.

**Структура:**
```
Assistant.API/
├── Controllers/
│   ├── ChatController.cs        # POST /api/chat
│   ├── TasksController.cs       # CRUD для задач
│   ├── RemindersController.cs   # CRUD для напоминаний
│   └── MemoryController.cs      # Управление памятью
│
├── Hubs/
│   └── AssistantHub.cs          # SignalR real-time hub
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   ├── LoggingMiddleware.cs
│   └── AuthMiddleware.cs
│
├── BackgroundJobs/
│   └── ReminderJob.cs           # Hangfire job для напоминаний
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── appsettings.json
└── Program.cs
```

**API Endpoints:**
```
POST   /api/chat              # Отправить сообщение
GET    /api/chat/history      # История сообщений

GET    /api/tasks             # Список задач
POST   /api/tasks             # Создать задачу
PUT    /api/tasks/{id}        # Обновить задачу
DELETE /api/tasks/{id}        # Удалить задачу

GET    /api/reminders         # Список напоминаний
POST   /api/reminders         # Создать напоминание
PUT    /api/reminders/{id}    # Обновить напоминание
DELETE /api/reminders/{id}    # Удалить напоминание

GET    /api/memory/search     # Поиск в памяти
POST   /api/memory/store      # Сохранить в память

GET    /api/preferences       # Получить настройки
PUT    /api/preferences       # Обновить настройки
```

---

### 3. **Assistant.Core** (Business Layer)

**Назначение:** Бизнес-логика, domain models, интерфейсы.

**Структура:**
```
Assistant.Core/
├── Entities/                # Domain entities
│   ├── Message.cs
│   ├── Task.cs
│   ├── Reminder.cs
│   ├── Memory.cs
│   └── UserPreference.cs
│
├── Interfaces/              # Abstractions
│   ├── IRepository.cs
│   ├── ITaskRepository.cs
│   ├── IReminderRepository.cs
│   ├── IMemoryRepository.cs
│   ├── IAIService.cs
│   ├── IAgentRouter.cs
│   └── IVectorStore.cs
│
├── Services/                # Business logic
│   ├── TaskService.cs
│   ├── ReminderService.cs
│   ├── MemoryService.cs
│   └── ConversationService.cs
│
├── Enums/
│   ├── MessageRole.cs       # User, Assistant, System
│   ├── TaskPriority.cs      # Low, Medium, High
│   └── AgentType.cs         # Command, Task, Query, Memory
│
└── Exceptions/
    ├── NotFoundException.cs
    └── ValidationException.cs
```

**Domain Models:**

```csharp
// Message.cs
public class Message
{
    public Guid Id { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; }
    public Guid SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Task.cs
public class TaskEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Reminder.cs
public class Reminder
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime RemindAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Memory.cs
public class Memory
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public float[] Embedding { get; set; }  // Vector для поиска
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### 4. **Assistant.Infrastructure** (Data & External Services)

**Назначение:** Реализация интерфейсов, работа с БД, AI, внешними API.

**Структура:**
```
Assistant.Infrastructure/
├── Data/
│   ├── AssistantDbContext.cs    # EF Core DbContext
│   ├── Configurations/          # Entity configurations
│   └── Migrations/              # EF Migrations
│
├── Repositories/
│   ├── Repository.cs            # Generic repository
│   ├── TaskRepository.cs
│   ├── ReminderRepository.cs
│   └── MemoryRepository.cs
│
├── AI/
│   ├── OpenAIService.cs         # OpenAI API client
│   ├── WhisperService.cs        # Speech-to-Text
│   ├── TTSService.cs            # Text-to-Speech
│   └── EmbeddingService.cs      # Vector embeddings
│
├── Agents/                      # AI Agents
│   ├── IAgent.cs
│   ├── CommandRouterAgent.cs    # Определяет intent
│   ├── TaskAgent.cs             # Управление задачами
│   ├── ReminderAgent.cs         # Напоминания
│   ├── QueryAgent.cs            # Вопросы и ответы
│   └── MemoryAgent.cs           # Контекстная память
│
├── Services/
│   ├── CacheService.cs          # Redis caching
│   ├── VectorStoreService.cs    # pgvector для поиска
│   └── NotificationService.cs   # Push notifications
│
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## 🤖 AI Агентная архитектура

### Agent Flow

```
User Input (Text/Voice)
         ↓
    [STT Service]  ← если голос
         ↓
┌────────────────────────┐
│  Command Router Agent  │  ← определяет intent
│  (Semantic Kernel)     │
└───────────┬────────────┘
            │
    ┌───────┴────────┬──────────┬──────────┬──────────┐
    │                │          │          │          │
┌───▼────┐  ┌───────▼──┐  ┌────▼───┐  ┌───▼────┐  ┌──▼─────┐
│ Task   │  │ Reminder │  │ Query  │  │ Memory │  │ Other  │
│ Agent  │  │ Agent    │  │ Agent  │  │ Agent  │  │ ...    │
└───┬────┘  └───────┬──┘  └────┬───┘  └───┬────┘  └──┬─────┘
    │               │          │          │          │
    └───────────────┴──────────┴──────────┴──────────┘
                            ↓
                    [Response Generator]
                            ↓
                     [TTS Service]  ← если нужно
                            ↓
                    User Output (Text/Voice)
```

### Агенты в деталях

#### 1. **Command Router Agent**
```csharp
public class CommandRouterAgent : IAgent
{
    // Анализирует запрос и определяет какой агент нужен
    // Использует GPT для classification:
    
    Примеры:
    "добавь задачу купить молоко" → TaskAgent
    "что у меня на сегодня?" → TaskAgent + QueryAgent
    "напомни через час" → ReminderAgent
    "объясни что такое REST" → QueryAgent
    "помнишь мы обсуждали X?" → MemoryAgent
}
```

#### 2. **Task Agent**
```csharp
public class TaskAgent : IAgent
{
    // CRUD операции с задачами
    // Парсинг natural language в structured data
    
    Примеры:
    "добавь задачу: купить молоко завтра" 
      → создает Task { title: "купить молоко", dueDate: tomorrow }
    
    "покажи незавершенные задачи"
      → возвращает список active tasks
    
    "отметь задачу X как выполненную"
      → updates task.IsCompleted = true
}
```

#### 3. **Reminder Agent**
```csharp
public class ReminderAgent : IAgent
{
    // Создание и управление напоминаниями
    // Парсинг времени (Chronic.NET или custom)
    
    Примеры:
    "напомни через 40 минут про встречу"
      → создает Reminder { remindAt: now + 40min }
    
    "напоминай мне каждый день в 9 утра"
      → recurring reminder
    
    "покажи активные напоминания"
      → список pending reminders
}
```

#### 4. **Query Agent**
```csharp
public class QueryAgent : IAgent
{
    // Ответы на вопросы через GPT
    // Поиск информации
    
    Примеры:
    "объясни что такое Clean Architecture"
      → GPT response
    
    "как приготовить пасту карбонара?"
      → GPT response
    
    "что означает код ошибки X?"
      → GPT response + возможно поиск в памяти
}
```

#### 5. **Memory Agent**
```csharp
public class MemoryAgent : IAgent
{
    // Долгосрочная память через pgvector
    // Semantic search по истории
    
    Примеры:
    "помнишь мы обсуждали проект X?"
      → vector search в Memory table
    
    "что я говорил про свои предпочтения?"
      → retrieval from memory
    
    // Автоматически сохраняет важную информацию
}
```

---

## 💾 Database Schema

### PostgreSQL Tables

```sql
-- Messages (история чата)
CREATE TABLE messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role VARCHAR(20) NOT NULL,  -- 'user' | 'assistant' | 'system'
    content TEXT NOT NULL,
    session_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_session (session_id),
    INDEX idx_created (created_at)
);

-- Tasks (задачи)
CREATE TABLE tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(500) NOT NULL,
    description TEXT,
    is_completed BOOLEAN DEFAULT false,
    priority INT DEFAULT 1,  -- 1: Low, 2: Medium, 3: High
    due_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP,
    
    INDEX idx_completed (is_completed),
    INDEX idx_due_date (due_date)
);

-- Reminders (напоминания)
CREATE TABLE reminders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(500) NOT NULL,
    remind_at TIMESTAMP NOT NULL,
    is_completed BOOLEAN DEFAULT false,
    is_recurring BOOLEAN DEFAULT false,
    recurrence_pattern VARCHAR(100),  -- 'daily', 'weekly', 'monthly'
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_remind_at (remind_at),
    INDEX idx_completed (is_completed)
);

-- Memory (долгосрочная память с векторами)
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE memory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content TEXT NOT NULL,
    embedding vector(1536),  -- OpenAI ada-002 embeddings
    metadata JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_created (created_at)
);

-- Vector similarity search index
CREATE INDEX ON memory USING ivfflat (embedding vector_cosine_ops);

-- User Preferences (настройки)
CREATE TABLE user_preferences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key VARCHAR(100) NOT NULL UNIQUE,
    value TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 🔄 Data Flow Examples

### Example 1: Голосовой запрос на создание задачи

```
1. User: [Нажимает кнопку микрофона в MAUI app]
2. MAUI AudioService: записывает аудио → отправляет на API
3. API WhisperService: audio → "добавь задачу купить молоко завтра"
4. CommandRouterAgent: определяет intent → TaskAgent
5. TaskAgent: 
   - парсит: title="купить молоко", dueDate=tomorrow
   - создает Task в БД
   - возвращает: "Задача 'купить молоко' добавлена на завтра"
6. API TTSService: text → audio
7. MAUI App: воспроизводит аудио + показывает текст
```

### Example 2: Вопрос с использованием памяти

```
1. User: "помнишь мы обсуждали проект X?"
2. CommandRouterAgent: → MemoryAgent
3. MemoryAgent:
   - создает embedding для запроса
   - ищет похожие в БД (pgvector similarity search)
   - находит релевантные сообщения из прошлого
   - передает контекст в GPT
4. QueryAgent + GPT: генерирует ответ с учетом найденного контекста
5. Response: "Да, мы обсуждали проект X 3 дня назад. Ты говорил что..."
```

---

## 🚀 Deployment Architecture

```
┌─────────────────────────────────────────────┐
│              Client Devices                 │
│  iOS │ Android │ Windows │ macOS            │
└──────────────────┬──────────────────────────┘
                   │ HTTPS
                   │
┌──────────────────▼──────────────────────────┐
│             Railway Platform                │
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │   ASP.NET Core Web API              │   │
│  │   (Docker Container)                │   │
│  └───────────┬─────────────────────────┘   │
│              │                              │
│  ┌───────────▼──────────┐  ┌────────────┐  │
│  │  PostgreSQL + vector │  │   Redis    │  │
│  └──────────────────────┘  └────────────┘  │
└─────────────────────────────────────────────┘
                   │
                   │ HTTPS
                   │
┌──────────────────▼──────────────────────────┐
│              OpenAI API                     │
│   GPT-4 │ Whisper │ TTS │ Embeddings       │
└─────────────────────────────────────────────┘
```

---

## 🔐 Security

- JWT токены для аутентификации (опционально для v2)
- HTTPS only
- API rate limiting
- Input validation & sanitization
- Secrets в environment variables
- OpenAI API key в backend only (не в клиенте)

---

## 📈 Performance Optimization

1. **Caching (Redis):**
   - Частые запросы к GPT
   - Embeddings для популярных запросов
   - User preferences

2. **Database:**
   - Indexes на часто запрашиваемые поля
   - Connection pooling
   - Pagination для больших списков

3. **AI:**
   - Streaming responses (SignalR)
   - Batching для embeddings
   - gpt-4o-mini для быстрых запросов

4. **Client:**
   - Local SQLite кэш
   - Offline mode для чтения
   - Lazy loading

---

## 🧪 Testing Strategy

```
Unit Tests:
- Assistant.Core (business logic)
- Agents logic

Integration Tests:
- API endpoints
- Database operations
- OpenAI integration

E2E Tests:
- MAUI UI tests
- Critical user flows

Performance Tests:
- API response time
- Database queries
- Concurrent users
```

---

## 📊 Monitoring & Logging

- **Serilog** для структурированного логирования
- **Application Insights** (опционально)
- Логи всех AI запросов (для отладки и оптимизации)
- Performance metrics
- Error tracking

---

Это полная архитектура проекта! Готов начать имплементацию? 🚀
