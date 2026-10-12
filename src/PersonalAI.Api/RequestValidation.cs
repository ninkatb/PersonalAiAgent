using System.Text.Json;

namespace PersonalAI.Api;

public static class RequestValidation
{
    public static async Task<T> ReadAsync<T>(HttpContext context, Func<T, bool> isValid) where T : class
    {
        var value = await JsonSerializer.DeserializeAsync<T>(context.Request.Body,
            SessionConfiguration.JsonOptions, context.RequestAborted);
        if (value is null || !isValid(value)) throw new InvalidDataException("VALIDATION_FAILED");
        return value;
    }
}
