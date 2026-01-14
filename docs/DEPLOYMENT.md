# 🚀 Deployment на Railway

## Что такое Railway?

Railway - это PaaS платформа для деплоя приложений. Идеально подходит для нашего ассистента:
- ✅ Автоматический деплой из Git
- ✅ Встроенный PostgreSQL
- ✅ Встроенный Redis
- ✅ Бесплатный tier (с ограничениями)

---

## 1️⃣ Подготовка проекта

### Создай Dockerfile для Backend:

Создай файл `src/Assistant.API/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Assistant.API/Assistant.API.csproj", "Assistant.API/"]
COPY ["src/Assistant.Core/Assistant.Core.csproj", "Assistant.Core/"]
COPY ["src/Assistant.Infrastructure/Assistant.Infrastructure.csproj", "Assistant.Infrastructure/"]
COPY ["src/Assistant.Shared/Assistant.Shared.csproj", "Assistant.Shared/"]

RUN dotnet restore "Assistant.API/Assistant.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Assistant.API"
RUN dotnet build "Assistant.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Assistant.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Assistant.API.dll"]
```

### Создай `.dockerignore`:

```
**/bin
**/obj
**/logs
**/*.Development.json
**/appsettings.Development.json
.git
.vs
.vscode
*.md
```

---

## 2️⃣ Setup Railway

### 1. Зарегистрируйся на Railway:

https://railway.app/

### 2. Создай новый проект:

1. Нажми "New Project"
2. Выбери "Deploy from GitHub repo"
3. Подключи свой GitHub аккаунт
4. Выбери репозиторий AssistantProject

### 3. Добавь PostgreSQL:

1. В проекте нажми "+ New"
2. Выбери "Database" → "PostgreSQL"
3. Railway автоматически создаст БД и выдаст connection string

### 4. Добавь Redis (опционально):

1. В проекте нажми "+ New"
2. Выбери "Database" → "Redis"

### 5. Настрой переменные окружения:

В настройках Backend сервиса добавь:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080

# Railway автоматически добавит DATABASE_URL
# Но мы переназначим его в правильный формат:
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
ConnectionStrings__Redis=${{Redis.REDIS_URL}}

OpenAI__ApiKey=sk-your-openai-key
OpenAI__Model=gpt-4o-mini
OpenAI__WhisperModel=whisper-1
OpenAI__TTSModel=tts-1
OpenAI__EmbeddingModel=text-embedding-ada-002
```

---

## 3️⃣ Настройка Production конфига

### Обнови `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### Создай `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## 4️⃣ Применение миграций на Railway

### Вариант 1: Через Railway CLI (рекомендуется)

```bash
# Установи Railway CLI
npm install -g @railway/cli

# Залогинься
railway login

# Подключись к проекту
railway link

# Примени миграции
railway run dotnet ef database update --project src/Assistant.Infrastructure
```

### Вариант 2: Автоматические миграции при старте

Добавь в `Program.cs` перед `app.Run()`:

```csharp
// Auto-migrate on startup (только для production!)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
    context.Database.Migrate();
    
    Log.Information("Database migrations applied successfully");
}
```

⚠️ **Внимание:** Автоматические миграции удобны, но рискованны для production!

---

## 5️⃣ Deployment Process

### При каждом push в main:

1. Railway автоматически:
   - Скачивает код
   - Собирает Docker образ
   - Применяет миграции (если настроено)
   - Деплоит новую версию
   - Делает health check

2. Получишь уникальный URL типа:
   ```
   https://your-app-name.railway.app
   ```

---

## 6️⃣ Мониторинг

### Railway Dashboard:

- **Metrics:** CPU, Memory, Network
- **Logs:** Real-time логи
- **Deployments:** История деплоев
- **Variables:** Управление env переменными

### Health Check:

Настрой в Railway:
```
Path: /health
Interval: 30s
Timeout: 10s
```

---

## 7️⃣ Custom Domain (опционально)

1. В настройках проекта → Settings
2. Добавь свой домен
3. Настрой DNS записи
4. Railway автоматически выпустит SSL сертификат

---

## 8️⃣ Масштабирование

### Вертикальное:

В Railway можно увеличить:
- CPU: до 8 vCPU
- RAM: до 32 GB

### Горизонтальное:

Railway поддерживает автоматическое масштабирование (платный план)

---

## 9️⃣ Стоимость

### Бесплатный tier:
- $5 бесплатных кредитов в месяц
- Достаточно для тестирования и небольших проектов

### Hobby план ($5/месяц):
- $5 + usage
- Подходит для личного использования

### Оценка расходов для нашего проекта:
- Backend: ~$3-5/месяц
- PostgreSQL: Включено
- Redis: Включено
- OpenAI API: Отдельно (зависит от использования)

**Общая стоимость:** ~$5-10/месяц (без учета OpenAI)

---

## 🔒 Security Best Practices

1. **Никогда не коммить API ключи в Git**
   - Используй переменные окружения
   - Добавь `appsettings.Development.json` в `.gitignore`

2. **Используй секреты Railway**
   - Все чувствительные данные храни в Railway variables

3. **Enable HTTPS only**
   - Railway автоматически настроит SSL

4. **Rate limiting**
   - Добавь middleware для защиты от DDoS

---

## 📊 Мониторинг и Алерты

### Sentry (опционально):

1. Зарегистрируйся на https://sentry.io
2. Добавь NuGet пакет:
   ```bash
   dotnet add package Sentry.AspNetCore
   ```
3. Настрой в Program.cs:
   ```csharp
   builder.WebHost.UseSentry(o =>
   {
       o.Dsn = "your-sentry-dsn";
       o.Environment = builder.Environment.EnvironmentName;
   });
   ```

---

## 🚀 CI/CD Pipeline (опционально)

### GitHub Actions:

Создай `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Railway

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build
        run: dotnet build
      
      - name: Test
        run: dotnet test
      
      - name: Deploy to Railway
        run: |
          npm install -g @railway/cli
          railway up
```

---

**Готово! Твой AI ассистент теперь в облаке! ☁️🚀**
