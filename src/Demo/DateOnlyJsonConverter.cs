﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo;
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const String Format = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.ParseExact(reader.GetString()!, Format);
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}