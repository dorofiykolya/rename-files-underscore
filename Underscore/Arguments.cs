using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Underscore
{
    public class Arguments
    {
        public delegate void ExistKey(string key, string value);

        private readonly Dictionary<string, string> _map;
        private readonly string _verboseKey;
        private readonly List<string> _pathList;

        public Arguments(Dictionary<string, string> map, List<string> pathList, string verboseKey)
        {
            _map = map;
            _pathList = pathList;
            _verboseKey = verboseKey;
        }

        public bool IsVerbose
        {
            get => Contains(_verboseKey);
            set
            {
                if (value) _map[_verboseKey] = "true";
                else _map.Remove(_verboseKey);
            }
        }

        public string this[string key]
        {
            get => _map[key];
            set => _map[key] = value;
        }

        public string[] Paths => _pathList.ToArray();
        public int PathsCount => _pathList.Count;

        public bool Remove(string key)
        {
            return _map.Remove(key);
        }

        public bool Contains(string key)
        {
            return _map.ContainsKey(key);
        }

        public void OnExist(string key, ExistKey callback)
        {
            if (Contains(key))
            {
                callback(key, this[key]);
            }
        }

        public void AssertKey(string key, string message = null)
        {
            if (message == null)
            {
                message = "command line build required argument:{0}";
            }

            Debug.Assert(_map.ContainsKey(key), string.Format(message, key));
        }

        public void AssertKeys(params string[] keys)
        {
            var required = new List<string>();
            foreach (var key in keys)
            {
                if (!_map.ContainsKey(key))
                {
                    required.Add(key);
                }
            }

            var requiredKeys = string.Join(", ", required.ToArray());
            var message = "command line build required arguments:{0}";

            Debug.Assert(required.Count == 0, string.Format(message, requiredKeys));
        }

        public bool GetAsBool(string key, bool falseIfNotExist = true)
        {
            if (falseIfNotExist && !Contains(key))
            {
                return false;
            }

            bool boolResult = true;
            var boolValue = this[key];
            if (!string.IsNullOrEmpty(boolValue))
            {
                boolValue = boolValue.Trim().ToLowerInvariant();
                if (boolValue.Length != 0)
                {
                    if (boolValue == "false") boolResult = false;
                    if (boolValue == "0") boolResult = false;
                    if (boolValue == "null") boolResult = false;
                    if (boolValue == "no") boolResult = false;
                    if (boolValue == "none") boolResult = false;
                }
            }

            return boolResult;
        }

        public int GetAsInt(string key)
        {
            var isOk = int.TryParse(this[key], NumberStyles.Any, CultureInfo.InvariantCulture, out var value);
            Debug.Assert(isOk, Utils.GetMessageNotValidArgs(key, this, new[] {"int (example: 10)"}));
            return value;
        }

        public float GetAsFloat(string key)
        {
            var isOk = float.TryParse(this[key], NumberStyles.Any, CultureInfo.InvariantCulture, out var value);
            Debug.Assert(isOk, Utils.GetMessageNotValidArgs(key, this, new[] {"float (example: 1.4)"}));
            return value;
        }

        public T Fill<T>() where T : new()
        {
            var args = this;
            var type = typeof(T);
            var result = new T();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField |
                                        BindingFlags.SetField);
            foreach (var field in fields)
            {
                if (args.Contains(field.Name))
                {
                    if (field.FieldType.IsPrimitive)
                    {
                        if (field.FieldType == typeof(bool))
                        {
                            bool boolResult = GetAsBool(field.Name);
                            field.SetValue(result, boolResult);
                        }
                        else
                        {
                            field.SetValue(result, Convert.ChangeType(args[field.Name], field.FieldType));
                        }
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(result, args[field.Name]);
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        object enumValue = Enum.Parse(field.FieldType, args[field.Name]);
                        field.SetValue(result, enumValue);
                    }
                    else
                    {
                        throw new ArgumentException($"type {field.FieldType} not supported");
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var pair in _map)
            {
                builder.Append(pair.Key);

                if (!string.IsNullOrEmpty(pair.Value))
                {
                    builder.Append('=');
                    builder.Append(pair.Value);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        public string ToString(string format)
        {
            var builder = new StringBuilder();

            foreach (var pair in _map)
            {
                builder.AppendFormat(format, pair.Key, pair.Value);
            }

            return builder.ToString();
        }

        public T GetValueByEnum<T>(string key) where T : struct
        {
            string value = this[key];
            T enumValue;
            if (Utils.TryParse<T>(value, out enumValue))
            {
                return enumValue;
            }

            throw new ArgumentException($"invalid '{key}' command value:'{value}'");
        }

        public bool TryGetValueByEnum<T>(string key, out T result) where T : struct
        {
            if (Contains(key))
            {
                result = GetValueByEnum<T>(key);
                return true;
            }

            result = default(T);
            return false;
        }
    }
}