using System;
using System.Xml.Linq;
using Spreedly.Net.Model;

namespace Spreedly.Net.Extensions
{
    public static class SpreedlyExtensions
    {
        public static bool Failed<T>(this AsyncCallResult<T> result) where T : class
        {
            return result == null || result.FailureReason != AsyncCallFailureReason.None;
        }

        public static string GetStringChild(this XElement node_, string name)
        {
            return GetStringChild(node_, name, string.Empty);
        }

        public static string GetStringChild(this XElement node_, string name, string defaultValue)
        {
            if (node_ == null) return defaultValue;
            var token = node_.Element(name);
            return token == null ? defaultValue : token.Value;
        }

    }
}