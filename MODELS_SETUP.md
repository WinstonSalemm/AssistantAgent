# ✅ Переключение моделей GPT реализовано!

## 🎯 Что сделано:

### 1. Базовая модель (gpt-5-mini) - по умолчанию
- Используется для всех обычных запросов
- Быстро и дешево
- Установлена как дефолт в конфигурации

### 2. Модель "думай глубже" (gpt-5.2)
- Используется когда `useDeepThinking: true` в запросе
- Более глубокий анализ и размышления
- Активируется кнопкой "Думай глубже" в UI

---

## 📝 Изменения в коде:

### ChatRequest (DTO):
```csharp
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public bool IsVoice { get; set; } = false;
    public bool UseDeepThinking { get; set; } = false; // ← НОВОЕ!
}
```

### ChatController:
- Определяет модель на основе `UseDeepThinking`
- Передает модель через контекст в агенты
- gpt-5-mini если `false`, gpt-5.2 если `true`

### QueryAgent:
- Использует модель из контекста если указана
- Иначе использует дефолтную (gpt-5-mini)

---

## 🚀 Как использовать:

### Обычный запрос (gpt-5-mini):
```json
POST /api/chat
{
  "message": "объясни что такое REST API",
  "useDeepThinking": false
}
```

### Запрос "думай глубже" (gpt-5.2):
```json
POST /api/chat
{
  "message": "объясни что такое REST API",
  "useDeepThinking": true
}
```

---

## ⚙️ Конфигурация Railway:

**ВАЖНО:** Добавь в Railway Variables:

```env
OpenAI__ApiKey=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
OpenAI__Model=gpt-5-mini
OpenAI__DeepThinkingModel=gpt-5.2
```

---

## 📱 В MAUI App:

Когда пользователь нажимает "Думай глубже":

```csharp
var request = new ChatRequest
{
    Message = userInput,
    UseDeepThinking = true  // ← Включает gpt-5.2
};

var response = await apiService.SendMessageAsync(request);
```

---

## ✅ Готово к использованию!

После пуша в GitHub и обновления переменных в Railway всё заработает автоматически!

**Следующий шаг:** Запушить изменения и обновить Railway Variables.
