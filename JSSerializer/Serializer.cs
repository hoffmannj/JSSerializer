using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JSSerializer
{
    public class Serializer : ISerializer
    {
        private delegate string SerializerFunction(object obj, Stack<object> chain);

        private Dictionary<Type, SerializerFunction> serializerMap = new Dictionary<Type, SerializerFunction>();

        public Serializer()
        {
            serializerMap[typeof(char)] = SerializeValueAsString;
            serializerMap[typeof(string)] = SerializeValueAsString;
            serializerMap[typeof(Guid)] = SerializeValueAsString;
            serializerMap[typeof(byte)] = SerializeSingleValue;
            serializerMap[typeof(int)] = SerializeSingleValue;
            serializerMap[typeof(long)] = SerializeSingleValue;
            serializerMap[typeof(short)] = SerializeSingleValue;
            serializerMap[typeof(uint)] = SerializeSingleValue;
            serializerMap[typeof(ulong)] = SerializeSingleValue;
            serializerMap[typeof(decimal)] = SerializeSingleValue;
            serializerMap[typeof(double)] = SerializeSingleValue;
            serializerMap[typeof(float)] = SerializeSingleValue;
            serializerMap[typeof(bool)] = SerializeBoolValue;
            serializerMap[typeof(DateTime)] = SerializeDateTimeValue;
            serializerMap[typeof(TimeSpan)] = SerializeValueAsString;
        }

        public string Serialize(object obj)
        {
            return Serialize(obj, new Stack<object>());
        }

        private string Serialize(object obj, Stack<object> chain)
        {
            if (chain.Contains(obj)) throw new Exception("Circular references found");
            Type t = obj == null ? null : obj.GetType();
            chain.Push(obj);
            var re = GetSerializerFor(obj, t)(obj, chain);
            chain.Pop();
            return re;
        }

        private SerializerFunction GetSerializerFor(object obj, Type objType)
        {
            if (obj == null) return SerializeNull;
            SerializerFunction serializerFunc;
            if (serializerMap.TryGetValue(objType, out serializerFunc)) return serializerFunc;
            if (obj is Type) return SerializeTypeValue;
            if (obj is DictionaryEntry) return SerializeDictionaryEntryValue;
            if (obj is IDictionary) return SerializeDictionary;
            if (obj is IEnumerable) return SerializeArray;
            return SerializeObjectValue;
        }

        private string SerializeNull(object obj, Stack<object> chain)
        {
            return "null";
        }

        private string SerializeSingleValue(object obj, Stack<object> chain)
        {
            return obj.ToString();
        }

        private string SerializeBoolValue(object obj, Stack<object> chain)
        {
            return (bool)obj ? "true" : "false";
        }

        private string SerializeValueAsString(object obj, Stack<object> chain)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(obj.ToString().Replace("\"", "\\\""));
            sb.Append("\"");
            return sb.ToString();
        }

        private string SerializeDateTimeValue(object obj, Stack<object> chain)
        {
            var dt = (DateTime)obj;
            return SerializeValueAsString(dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), chain);
        }

        private string SerializeTypeValue(object obj, Stack<object> chain)
        {
            return SerializeValueAsString((obj as Type).AssemblyQualifiedName, chain);
        }

        private string SerializeDictionaryEntryValue(object obj, Stack<object> chain)
        {
            var de = (DictionaryEntry)obj;
            var sb = new StringBuilder();
            sb.Append(ObjectToStringValue(de.Key, chain));
            sb.Append(":");
            sb.Append(Serialize(de.Value, chain));
            return sb.ToString();
        }

        private string SerializeArray(object obj, Stack<object> chain)
        {
            var e = obj as IEnumerable;
            var sb = new StringBuilder();
            var idx = 0;
            sb.Append("[");
            foreach (var item in e)
            {
                if (idx > 0)
                {
                    sb.Append(",");
                }
                sb.Append(Serialize(item, chain));
                ++idx;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string SerializeDictionary(object obj, Stack<object> chain)
        {
            var e = obj as IDictionary;
            var sb = new StringBuilder();
            var idx = 0;
            sb.Append("{");
            foreach (DictionaryEntry item in e)
            {
                if (idx > 0)
                {
                    sb.Append(",");
                }
                sb.Append(Serialize(item, chain));
                ++idx;
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string ObjectToStringValue(object obj, Stack<object> chain)
        {
            var str = Serialize(obj, chain);
            if (!str.StartsWith("\"")) return SerializeValueAsString(str, chain);
            return str;
        }

        private string SerializeObjectValue(object obj, Stack<object> chain)
        {
            var sb = new StringBuilder();
            var idx = 0;
            var objType = obj.GetType();
            var members = objType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            sb.Append("{");
            foreach (var member in members)
            {
                if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property) continue;
                if (idx > 0)
                {
                    sb.Append(",");
                }
                sb.Append(Serialize(new DictionaryEntry(member.Name, GetMemberValue(member, obj)), chain));
                ++idx;
            }
            sb.Append("}");
            return sb.ToString();
        }

        private object GetMemberValue(MemberInfo memberInfo, object obj)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                var fieldInfo = memberInfo as FieldInfo;
                return fieldInfo.GetValue(obj);
            }
            else
            {
                var propertyInfo = memberInfo as PropertyInfo;
                return propertyInfo.GetValue(obj);
            }
        }
    }
}
