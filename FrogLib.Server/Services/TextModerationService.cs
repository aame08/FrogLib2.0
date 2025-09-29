using System.Text;
using System.Text.Json;

namespace FrogLib.Server.Services
{
    public interface ITextModerationService
    {
        Task<bool> CheckTextAsync(string text);
        Task<string> HighlightTextAsync(string text);
    }

    public class TextModerationService(HttpClient httpClient) : ITextModerationService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<bool> CheckTextAsync(string text)
        {
            var payload = JsonSerializer.Serialize(new { text });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://127.0.0.1:8000/check_text", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("has_forbidden").GetBoolean();
        }

        public async Task<string> HighlightTextAsync(string text)
        {
            var payload = JsonSerializer.Serialize(new { text });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://127.0.0.1:8000/highlight_text", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("highlighted").GetString();
        }
    }
}
