namespace Assistant.Shared.Constants;

public static class ApiRoutes
{
    private const string BaseUrl = "api";
    
    public static class Chat
    {
        public const string Base = $"{BaseUrl}/chat";
        public const string Send = Base;
        public const string History = $"{Base}/history";
    }
    
    public static class Tasks
    {
        public const string Base = $"{BaseUrl}/tasks";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Active = $"{Base}/active";
        public const string Completed = $"{Base}/completed";
    }
    
    public static class Reminders
    {
        public const string Base = $"{BaseUrl}/reminders";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Active = $"{Base}/active";
    }
}
