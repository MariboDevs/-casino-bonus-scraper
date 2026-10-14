using System.Text.Encodings.Web;
using System.Text.Json;

namespace BonusScraper.Infrastructure.Extraction;

internal static class RecordingJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
