# 📝 Примеры POST запросов для API

## ✅ Правильные форматы запросов

---

## 1. POST /api/chat — Чат с AI

**URL:** `https://perceptive-perception-production.up.railway.app/api/chat`

### Обычный запрос (gpt-5-mini):
```json
{
  "message": "привет! как дела?",
  "useDeepThinking": false
}
```

### С "Думай глубже" (gpt-5.2):
```json
{
  "message": "объясни что такое REST API",
  "useDeepThinking": true
}
```

### С sessionId:
```json
{
  "message": "напомни что я спрашивал",
  "sessionId": "твой-guid-здесь",
  "useDeepThinking": false
}
```

**curl:**
```bash
curl -X POST https://perceptive-perception-production.up.railway.app/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "привет!", "useDeepThinking": false}'
```

---

## 2. POST /api/tasks — Создать задачу

**URL:** `https://perceptive-perception-production.up.railway.app/api/tasks`

### Минимальный запрос:
```json
{
  "title": "Тестовая задача",
  "priority": 2
}
```

### Полный запрос:
```json
{
  "title": "Купить молоко",
  "description": "Не забыть купить молоко в магазине",
  "priority": 1,
  "dueDate": "2026-01-15T18:00:00Z"
}
```

**Поля:**
- `title` (обязательно) — название задачи
- `description` (опционально) — описание
- `priority` (по умолчанию 2) — приоритет: 0=Low, 1=Medium, 2=High
- `dueDate` (опционально) — дата выполнения в формате ISO 8601

**curl:**
```bash
curl -X POST https://perceptive-perception-production.up.railway.app/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Тестовая задача", "priority": 2}'
```

---

## 3. POST /api/reminders — Создать напоминание

**URL:** `https://perceptive-perception-production.up.railway.app/api/reminders`

```json
{
  "title": "Встреча с командой",
  "remindAt": "2026-01-15T14:00:00Z",
  "isRecurring": false
}
```

**Поля:**
- `title` (обязательно) — название напоминания
- `remindAt` (обязательно) — время напоминания в формате ISO 8601
- `isRecurring` (по умолчанию false) — повторяющееся ли
- `recurrencePattern` (опционально) — паттерн повторения

**curl:**
```bash
curl -X POST https://perceptive-perception-production.up.railway.app/api/reminders \
  -H "Content-Type: application/json" \
  -d '{"title": "Встреча", "remindAt": "2026-01-15T14:00:00Z"}'
```

---

## ❌ Частые ошибки:

### Ошибка 1: Неправильные поля для endpoint
```json
// ❌ НЕПРАВИЛЬНО для /api/tasks:
{
  "message": "привет!",
  "useDeepThinking": false
}

// ✅ ПРАВИЛЬНО для /api/tasks:
{
  "title": "Тестовая задача",
  "priority": 2
}
```

### Ошибка 2: Неправильный endpoint
- `/api/chat` — для сообщений AI (поля: `message`, `useDeepThinking`)
- `/api/tasks` — для задач (поля: `title`, `priority`, `description`, `dueDate`)
- `/api/reminders` — для напоминаний (поля: `title`, `remindAt`)

---

## 🎯 Быстрая проверка:

### Тест БД (создать задачу):
```json
POST /api/tasks
{
  "title": "Тестовая задача",
  "priority": 2
}
```

### Тест AI (чат):
```json
POST /api/chat
{
  "message": "привет!",
  "useDeepThinking": false
}
```

---

**Готово! Используй правильные поля для каждого endpoint!** 🚀
