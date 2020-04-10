using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace HotCode.System
{
    public static class Extensions
    {
        public static T GetOptions<T>(this IConfiguration configuration, string section) where T : new()
        {
            var model = new T();
            configuration.GetSection(section).Bind(model);
            return model;
        }

        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()));

        public static string ToJson<T>(this T value) => JsonConvert.SerializeObject(value);
        public static T FromJson<T>(this RedisValue value) => JsonConvert.DeserializeObject<T>(value);
        public static T FromJson<T>(this string value) => JsonConvert.DeserializeObject<T>(value);

        public static T BindId<T>(this T model, Expression<Func<T, string>> expression)
            => model.Bind<T, string>(expression, Guid.NewGuid().ToString("D"));

        private static TModel Bind<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression,
            object value)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                memberExpression = ((UnaryExpression) expression.Body).Operand as MemberExpression;
            }

            if (memberExpression != null)
            {
                var propertyName = memberExpression.Member.Name.ToLowerInvariant();
                var modelType = model.GetType();
                var field = modelType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>"));
                if (field == null)
                {
                    return model;
                }

                field.SetValue(model, value);
            }

            return model;
        }

        public static DateTime UnixTimeStampToDate(this long unixTimeStamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, global::System.DateTimeKind.Utc);
            date = date.AddSeconds(unixTimeStamp);
            return date;
        }

        public static string IfEmptyThenNull(this string stringValue) =>
            string.IsNullOrEmpty(stringValue) ? null : stringValue;

        public static string IfEmptyThen(this string stringValue, string then) =>
            string.IsNullOrEmpty(stringValue) ? then : stringValue;
        
        public static string IfEmptyThenEmptyId(this string stringValue) =>
            string.IsNullOrEmpty(stringValue) ? "00000000-0000-0000-0000-000000000000" : stringValue;

        public static long GetUxTime(this DateTime dateTime)
        {
            return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
        }
    }
}