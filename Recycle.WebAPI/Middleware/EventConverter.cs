using System.Text.Json;
using System.Text.Json.Serialization;
using Recycle.WebAPI.Messages;

namespace Recycle.WebAPI.Middleware;

public class EventConverter : JsonConverter<Event>
{
    public override Event? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        // Read the `className` from our JSON document
        using var jsonDocument = JsonDocument.ParseValue(ref readerClone);
        var type = jsonDocument.RootElement.GetProperty("type").GetString();

        return type switch
        {
            "IdCardRegistered" => JsonSerializer.Deserialize<Event<IdCardRegistered>>(ref reader),
            "IdCardScannedAtEntranceGate" => JsonSerializer.Deserialize<Event<IdCardScannedAtEntranceGate>>(ref reader),
            "WeightWasMeasured" => JsonSerializer.Deserialize<Event<WeightWasMeasured>>(ref reader),
            "FractionWasSelected" => JsonSerializer.Deserialize<Event<FractionWasSelected>>(ref reader),
            "FractionWasDropped" => JsonSerializer.Deserialize<Event<FractionWasDropped>>(ref reader),
            "IdCardScannedAtExitGate" => JsonSerializer.Deserialize<Event<IdCardScannedAtExitGate>>(ref reader),
            "PriceWasCalculated" => JsonSerializer.Deserialize<Event<PriceWasCalculated>>(ref reader),
            _ => JsonSerializer.Deserialize<Event>(ref reader)
        };
    }

    public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case Event<PriceWasCalculated> priceWasCalculated:
                writer.WriteStartObject();

                writer.WriteString("type", "PriceWasCalculated");
                writer.WriteString("event_id", priceWasCalculated.EventId);
                writer.WriteString("created_at", priceWasCalculated.CreatedAt);

                writer.WritePropertyName("payload");
                writer.WriteRawValue(JsonSerializer.Serialize(priceWasCalculated.Payload, options));

                writer.WriteEndObject();

                break;
            default:
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
                break;
        }
    }
}