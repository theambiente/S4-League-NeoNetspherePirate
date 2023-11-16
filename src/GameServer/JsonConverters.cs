﻿using System;
using System.Net;
using Newtonsoft.Json;

namespace NeoNetsphere
{
  public class IPEndPointConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      serializer.Serialize(writer, value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
      var str = serializer.Deserialize<string>(reader);
      var arr = str.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
      return new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(IPEndPoint);
    }
  }

  public class TimeSpanConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      serializer.Serialize(writer, (uint)((TimeSpan)value).TotalMilliseconds);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
      var value = serializer.Deserialize<uint>(reader);
      return TimeSpan.FromMilliseconds(value);
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(TimeSpan);
    }
  }
}