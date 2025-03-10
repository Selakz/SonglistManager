using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SonglistManager.Models;
using System;
using System.Reflection;

namespace SonglistManager.Attributes
{
    public class CustomConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // 写的依托啊
            JObject jsonObject = JObject.Load(reader);
            try
            {
                object? result = Activator.CreateInstance<T>();
                if (result == null) return null;
                else if (result is SongInfo) result = new SongInfo();
                else if (result is SongInfo.Localization) result = new SongInfo.Localization();
                else if (result is SongInfo.BgDayNight) result = new SongInfo.BgDayNight();
                else if (result is SongInfo.Difficulty) result = new SongInfo.Difficulty();
                else return null;
                Type type = typeof(T);
                foreach (var property in type.GetProperties())
                {
                    if (property.IsDefined(typeof(JsonIgnoreAttribute), false)) continue;
                    string propertyName = property.Name;
                    var token = jsonObject[propertyName];
                    if (token is not null)
                    {
                        property.SetValue(result, token.ToObject(property.PropertyType));
                    }
                }
                return result;
            }
            catch { return null; }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null) return;
            writer.WriteStartObject();
            Type type = typeof(T);
            foreach (var property in type.GetProperties())
            {
                // NotRequired
                if (property.IsDefined(typeof(NotRequiredAttribute), false) && !ShouldSerialize(property, value)) continue;
                // JsonIgnore
                if (property.IsDefined(typeof(JsonIgnoreAttribute), false)) continue;

                string propertyName = property.Name;
                var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (attribute != null) propertyName = attribute.PropertyName ?? string.Empty;
                writer.WritePropertyName(propertyName);

                var propertyValue = property.GetValue(value);
                if (propertyValue != null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    serializer.Serialize(writer, propertyValue);
                }
                else
                {
                    writer.WriteValue(propertyValue);
                }
            }
            writer.WriteEndObject();
        }

        /// <summary> 在一个属性有[NotRequired]的情况下判断它是否应该被序列化 </summary>
        private static bool ShouldSerialize(PropertyInfo property, object? value)
        {
            var propertyValue = property.GetValue(value);
            if (propertyValue is null) return false;
            if (propertyValue is string str) return !string.IsNullOrEmpty(str);
            if (propertyValue is bool b) return b;
            if (propertyValue is double d) return d != 0;
            if (propertyValue is int i) return i != 0;
            if (propertyValue is SongInfo.Localization localization)
            {
                return !localization.IsEmpty();
            }
            if (propertyValue is SongInfo.BgDayNight bgDayNight)
            {
                if (string.IsNullOrEmpty(bgDayNight.day) && string.IsNullOrEmpty(bgDayNight.night)) return false;
            }
            return true;
        }
    }
}
