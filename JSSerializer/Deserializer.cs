using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSerializer
{
    public class Deserializer : IDeserializer
    {
        private enum JsonItemType
        {
            Empty,
            Null,
            Boolean,
            Number,
            String,
            Array,
            Dictionary
        }

        private enum JsonParserItemType
        {
            Null,
            Boolean,
            Number,
            String,
            Comma,
            Colon,
            ArrayStart,
            ArrayEnd,
            DictStart,
            DictEnd
        }

        private class JsonParserItem
        {
            public JsonParserItemType Type { get; set; }
            public string Value { get; set; }
        }

        private class JsonItem
        {
            public JsonItemType Type { get; set; }
            public string Value { get; set; }
            public bool BoolValue { get; set; }
            public List<JsonItem> ArrayItems { get; set; }
            public Dictionary<string, JsonItem> DictionaryItems { get; set; }

            public JsonItem()
            {
                Type = JsonItemType.Empty;
                ArrayItems = new List<JsonItem>();
                DictionaryItems = new Dictionary<string, JsonItem>();
            }
        }

        public T Deserialize<T>(string json)
        {
            var tType = typeof(T);
            json = json.Trim();
            var parsed = ParseJSon(json);
            return (T)GetParsedValueFromJsonItem(tType, parsed);
        }

        public dynamic DeserializeToDynamic(string json)
        {
            json = json.Trim();
            dynamic eo = new ExpandoObject();

            return eo;
        }

        private dynamic GetParsedValueFromJsonItem(Type toType, JsonItem item)
        {
            if (item.Type == JsonItemType.Null)
            {
                return null;
            }
            if (item.Type == JsonItemType.Boolean)
            {
                return item.BoolValue;
            }
            if (item.Type == JsonItemType.String)
            {
                return GetValueForStringJsonItem(toType, item);
            }
            if (item.Type == JsonItemType.Number)
            {
                return GetValueForNumberJsonItem(toType, item);
            }
            if (item.Type == JsonItemType.Array)
            {
                return GetValueForArrayJsonItem(toType, item);
            }
            if (item.Type == JsonItemType.Dictionary)
            {
                return GetValueForDictionaryJsonItem(toType, item);
            }
            return null;
        }

        private dynamic GetValueForStringJsonItem(Type toType, JsonItem item)
        {
            if (toType == typeof(Type)) return Type.GetType(item.Value);
            if (toType == typeof(string)) return item.Value;
            return GetObjectWithParse(toType, item.Value);
        }

        private dynamic GetValueForNumberJsonItem(Type toType, JsonItem item)
        {
            return GetObjectWithParse(toType, item.Value);
        }

        private dynamic GetValueForArrayJsonItem(Type toType, JsonItem item)
        {
            var itemType = typeof(object);
            if (toType.IsArray)
            {
                itemType = toType.GetElementType();
                dynamic aObj = Activator.CreateInstance(toType, item.ArrayItems.Count);
                for (int i = 0; i < item.ArrayItems.Count; ++i)
                {
                    aObj[i] = GetParsedValueFromJsonItem(itemType, item.ArrayItems[i]);
                }
                return aObj;
            }
            if (toType.IsGenericType)
            {
                itemType = toType.GenericTypeArguments.First();
            }
            var obj = Activator.CreateInstance(toType);
            var addMethod = toType.GetMethod("Add", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Type[] { itemType }, null);
            if (addMethod == null) throw new Exception("There's no Add method to use");
            for (int i = 0; i < item.ArrayItems.Count; ++i)
            {
                addMethod.Invoke(obj, new object[] { GetParsedValueFromJsonItem(itemType, item.ArrayItems[i]) });
            }
            return obj;
        }

        private dynamic GetValueForDictionaryJsonItem(Type toType, JsonItem item)
        {
            if (typeof(IDictionary).IsAssignableFrom(toType))
            {
                return CreateDictionaryObjectFromJsonItem(toType, item);
            }
            else
            {
                return CreateClassObjectFromJsonItem(toType, item);
            }
        }

        private dynamic CreateDictionaryObjectFromJsonItem(Type toType, JsonItem item)
        {
            var keyType = typeof(object);
            var valueType = keyType;
            var obj = Activator.CreateInstance(toType);
            if (toType.IsGenericType)
            {
                keyType = toType.GetGenericArguments()[0];
                valueType = toType.GetGenericArguments()[1];
            }
            var addMethod = toType.GetMethod("Add", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Type[] { keyType, valueType }, null);
            if (addMethod == null) throw new Exception("There's no Add method to use");
            foreach (var kv in item.DictionaryItems)
            {
                addMethod.Invoke(obj, new object[] {
                            GetValueForStringJsonItem(keyType, new JsonItem { Type = JsonItemType.String, Value = kv.Key}),
                            GetParsedValueFromJsonItem(valueType, kv.Value) });
            }
            return obj;
        }

        private dynamic CreateClassObjectFromJsonItem(Type toType, JsonItem item)
        {
            var obj = Activator.CreateInstance(toType);
            foreach (var kv in item.DictionaryItems)
            {
                var members = toType.GetMember(kv.Key);
                if (members != null)
                {
                    members = members.Where(m => m.MemberType == System.Reflection.MemberTypes.Field || m.MemberType == System.Reflection.MemberTypes.Property).ToArray();
                }
                if (members == null || members.Length == 0)
                {
                    throw new Exception(string.Format("Couldn't find member by the name of '{0}'", kv.Key));
                }
                var member = members[0];
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var fi = member as System.Reflection.FieldInfo;
                    fi.SetValue(obj, GetParsedValueFromJsonItem(fi.FieldType, kv.Value));
                }
                else
                {
                    var pi = member as System.Reflection.PropertyInfo;
                    if (pi.CanWrite)
                    {
                        pi.SetValue(obj, GetParsedValueFromJsonItem(pi.PropertyType, kv.Value));
                    }
                }
            }
            return obj;
        }

        private dynamic GetObjectWithParse(Type toType, string value)
        {
            var parseMethod = toType.GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            if (parseMethod != null)
            {
                return parseMethod.Invoke(null, new object[] { value });
            }
            return value;
        }

        private JsonItem ParseJSon(string json)
        {
            var items = TokenizeJson(json);
            var idx = 0;
            return ParseTokenizedJsonFrom(items, ref idx);
        }

        private JsonParserItemType[] ThrowIfTheseTypes = new[] {
                JsonParserItemType.ArrayEnd,
                JsonParserItemType.Colon,
                JsonParserItemType.Comma,
                JsonParserItemType.DictEnd
        };

        private JsonItem ParseTokenizedJsonFrom(List<JsonParserItem> items, ref int index)
        {
            var t = items[index];

            if (t.Type == JsonParserItemType.Null)
            {
                index++;
                return new JsonItem
                {
                    Type = JsonItemType.Null
                };
            }

            if (t.Type == JsonParserItemType.Boolean)
            {
                index++;
                return new JsonItem
                {
                    Type = JsonItemType.Boolean,
                    BoolValue = bool.Parse(t.Value),
                    Value = t.Value
                };
            }

            if (t.Type == JsonParserItemType.Number)
            {
                index++;
                return new JsonItem
                {
                    Type = JsonItemType.Number,
                    Value = t.Value
                };
            }

            if (t.Type == JsonParserItemType.String)
            {
                index++;
                return new JsonItem
                {
                    Type = JsonItemType.String,
                    Value = t.Value
                };
            }

            if (ThrowIfTheseTypes.Contains(t.Type))
            {
                return null;
            }

            if (t.Type == JsonParserItemType.ArrayStart)
            {
                var retVal = new JsonItem { Type = JsonItemType.Array };
                index++;
                while (true)
                {
                    var next = ParseTokenizedJsonFrom(items, ref index);
                    if (next != null)
                    {
                        retVal.ArrayItems.Add(next);
                        continue;
                    }
                    else if(items[index].Type == JsonParserItemType.Comma)
                    {
                        index++;
                        continue;
                    }
                    else if (items[index].Type == JsonParserItemType.ArrayEnd)
                    {
                        index++;
                        return retVal;
                    }
                    throw new Exception("Parsing error");
                }
            }

            if (t.Type == JsonParserItemType.DictStart)
            {
                var retVal = new JsonItem { Type = JsonItemType.Dictionary };
                index++;
                while (true)
                {
                    var key = ParseTokenizedJsonFrom(items, ref index);
                    if (key == null && items[index].Type == JsonParserItemType.DictEnd)
                    {
                        index++;
                        break;
                    }
                    if (key == null && items[index].Type == JsonParserItemType.Comma)
                    {
                        index++;
                        continue;
                    }

                    if (key == null)
                    {
                        throw new Exception("Parsing error");
                    }
                    if (items[index].Type != JsonParserItemType.Colon)
                    {
                        throw new Exception("Parsing error");
                    }
                    index++;
                    var value = ParseTokenizedJsonFrom(items, ref index);
                    if (value == null)
                    {
                        throw new Exception("Parsing error");
                    }

                    retVal.DictionaryItems.Add(key.Value, value);
                }
                return retVal;
            }
            throw new Exception("Parsing error");
        }

        private List<JsonParserItem> TokenizeJson(string json)
        {
            int pos = 0;
            List<JsonParserItem> items = new List<JsonParserItem>();
            var length = json.Length;
            while (pos < length)
            {
                SkipWhiteSpaceInJson(json, ref pos);
                var currentChar = char.ToLower(json[pos]);
                if (currentChar == '"')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.String,
                        Value = ReadNextJsonString(json, ref pos)
                    });
                    continue;
                }

                if (currentChar == '[')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.ArrayStart
                    });
                    pos++;
                    continue;
                }

                if (currentChar == ']')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.ArrayEnd
                    });
                    pos++;
                    continue;
                }

                if (currentChar == '{')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.DictStart
                    });
                    pos++;
                    continue;
                }

                if (currentChar == '}')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.DictEnd
                    });
                    pos++;
                    continue;
                }

                if (currentChar == ',')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.Comma
                    });
                    pos++;
                    continue;
                }

                if (currentChar == ':')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.Colon
                    });
                    pos++;
                    continue;
                }

                if (currentChar == 't')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.Boolean,
                        Value = ReadNextJsonBoolean(json, ref pos)
                    });
                    continue;
                }

                if (currentChar == 'f')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.Boolean,
                        Value = ReadNextJsonBoolean(json, ref pos)
                    });
                    continue;
                }

                if (currentChar == 'n')
                {
                    items.Add(new JsonParserItem
                    {
                        Type = JsonParserItemType.Null,
                        Value = ReadNextJsonNull(json, ref pos)
                    });
                    continue;
                }

                items.Add(new JsonParserItem
                {
                    Type = JsonParserItemType.Number,
                    Value = ReadNextJsonNumber(json, ref pos)
                });
                continue;
            }
            return items;
        }

        private void SkipWhiteSpaceInJson(string json, ref int pos)
        {
            var length = json.Length;
            while (pos < length)
            {
                if (!char.IsWhiteSpace(json[pos])) break;
                pos++;
            }
        }

        private string ReadNextJsonString(string json, ref int pos)
        {
            var level = 2;
            var sb = new StringBuilder();
            var prev = '\0';
            var length = json.Length;
            while (pos < length)
            {
                var ch = json[pos];
                pos++;
                if (ch == '"' && prev != '\\')
                {
                    level--;
                    if (level == 0) break;
                }
                else sb.Append(ch);
                prev = ch;
            }
            var retVal = sb.ToString();
            if (level != 0) throw new Exception("Wrong string format in JSON");
            return retVal;
        }

        private string ReadNextJsonNumber(string json, ref int pos)
        {
            var sb = new StringBuilder();
            var length = json.Length;
            while (pos < length)
            {
                var ch = json[pos];
                if (ch == ',' || ch == ']' || ch == '}') break;
                sb.Append(ch);
                pos++;
            }
            return sb.ToString();
        }

        private string ReadNextJsonBoolean(string json, ref int pos)
        {
            var startChar = json[pos];
            if (char.ToLower(startChar) == 'f')
            {
                pos += 5;
                return "false";
            }
            pos += 4;
            return "true";
        }

        private string ReadNextJsonNull(string json, ref int pos)
        {
            pos += 4;
            return "null";
        }
    }
}
