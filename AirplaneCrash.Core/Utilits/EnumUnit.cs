

namespace AirplaneCrash.Core.Utilits
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public static class EnumUnit
    {
        private static Dictionary<Type, Dictionary<object, string>> s_Cache = new Dictionary<Type, Dictionary<object, string>>();

        private static object s_SyncObj = new object();

        private static Dictionary<object, string> GetDescriptions(Type enumType)
        {
            if (!enumType.IsEnum && !IsGenericEnum(enumType))
            {
                throw new ApplicationException("The generic type 'TEnum' must be enum or Nullable<enum>.");
            }

            enumType = GetRealEnum(enumType);
            if (s_Cache.TryGetValue(enumType, out Dictionary<object, string> value))
            {
                return value;
            }

            lock (s_SyncObj)
            {
                if (s_Cache.TryGetValue(enumType, out value))
                {
                    return value;
                }

                FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
                Dictionary<object, string> dictionary = new Dictionary<object, string>(fields.Length * 2);
                FieldInfo[] array = fields;
                foreach (FieldInfo fieldInfo in array)
                {
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), inherit: false);
                    if (customAttributes != null && customAttributes.Length != 0 && !(customAttributes[0] as DisplayAttribute).Display)
                    {
                        continue;
                    }

                    object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
                    string value2 = string.Empty;
                    if (customAttributes2 != null && customAttributes2.Length != 0)
                    {
                        DescriptionAttribute descriptionAttribute = customAttributes2[0] as DescriptionAttribute;
                        if (descriptionAttribute != null && descriptionAttribute.Description != null)
                        {
                            value2 = descriptionAttribute.Description;
                        }
                    }

                    object value3 = fieldInfo.GetValue(null);
                    dictionary.Add(value3, value2);
                }

                s_Cache.Add(enumType, dictionary);
                return dictionary;
            }
        }

        private static string GetDescription(object enumValue, Type enumType)
        {
            Dictionary<object, string> descriptions = GetDescriptions(enumType);
            if (descriptions.TryGetValue(enumValue, out string value) && value != null)
            {
                return value;
                //return LangHelper.GetText(value, "enum\\" + enumType.Name, enumType.Module.Name);
            }

            return string.Empty;
        }

        private static bool IsGenericEnum(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments() != null && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum;
        }

        private static Type GetRealEnum(Type type)
        {
            Type type2 = type;
            while (IsGenericEnum(type2))
            {
                type2 = type.GetGenericArguments()[0];
            }

            return type2;
        }

        //
        // 摘要:
        //     获取枚举值的描述
        //
        // 参数:
        //   value:
        public static string GetDescription(this Enum value)
        {
            return (value == null) ? string.Empty : GetDescription(value, value.GetType());
        }

        //
        // 摘要:
        //     获取枚举内容项，并以KeyValuePair列表的方式返回；其中，Key=枚举的值，Value=枚举的描述
        //
        // 参数:
        //   appendType:
        //     选择是否在列表项前插入一条选项的选项类型，比如是否插入所有|请选择等Key为Null的选项
        //
        //   customApplyDesc:
        //     如果要在列表项前插入一条选项，该选项的自定义描述
        //
        // 类型参数:
        //   TEnum:
        //     枚举类型
        public static List<KeyValuePair<TEnum?, string>> GetKeyValuePairs<TEnum>(EnumAppendItemType appendType, params string[] customApplyDesc) where TEnum : struct
        {
            List<KeyValuePair<TEnum?, string>> list = new List<KeyValuePair<TEnum?, string>>();
            Type typeFromHandle = typeof(TEnum);
            if (typeFromHandle.IsEnum || IsGenericEnum(typeFromHandle))
            {
                Dictionary<TEnum, string> descriptions = GetDescriptions<TEnum>();
                if (descriptions != null && descriptions.Count > 0)
                {
                    foreach (TEnum key in descriptions.Keys)
                    {
                        list.Add(new KeyValuePair<TEnum?, string>(key, descriptions[key]));
                    }
                }

                if (appendType != 0)
                {
                    if (customApplyDesc != null && customApplyDesc.Length != 0 && !string.IsNullOrEmpty(customApplyDesc[0]))
                    {
                        KeyValuePair<TEnum?, string> item = new KeyValuePair<TEnum?, string>(null, customApplyDesc[0]);
                        list.Insert(0, item);
                    }
                    else
                    {
                        string description = appendType.GetDescription();
                        if (!string.IsNullOrEmpty(description))
                        {
                            KeyValuePair<TEnum?, string> item2 = new KeyValuePair<TEnum?, string>(null, description);
                            list.Insert(0, item2);
                        }
                    }
                }
            }

            return list;
        }

        public static List<dynamic> GetKeyValuePairs2<TEnum>()
        {
            List<object> list = new List<object>();
            Type typeFromHandle = typeof(TEnum);
            if (typeFromHandle.IsEnum || IsGenericEnum(typeFromHandle))
            {
                foreach (object value in Enum.GetValues(typeFromHandle))
                {
                    list.Add(new
                    {
                        value = (int)value,
                        text = GetDescription(value, typeFromHandle)
                    });
                }
            }

            return list;
        }

        //
        // 摘要:
        //     获取枚举类型下所有枚举值的描述集合
        //
        // 类型参数:
        //   TEnum:
        //     枚举类型
        //
        // 返回结果:
        //     枚举值与其描述的对应关系的集合，Key为枚举值，Value为其对应的描述
        public static Dictionary<TEnum, string> GetDescriptions<TEnum>() where TEnum : struct
        {
            Dictionary<object, string> descriptions = GetDescriptions(typeof(TEnum));
            Dictionary<TEnum, string> dictionary = new Dictionary<TEnum, string>(descriptions.Count * 2);
            foreach (KeyValuePair<object, string> item in descriptions)
            {
                dictionary.Add((TEnum)item.Key, item.Value);
                //dictionary.Add((TEnum)item.Key, LangHelper.GetText(item.Value, "enum\\" + typeof(TEnum).Name, typeof(TEnum).Module.Name));
            }

            return dictionary;
        }
    }

    public class DisplayAttribute : Attribute
    {
        private bool display;

        public bool Display
        {
            get
            {
                return display;
            }
            set
            {
                display = value;
            }
        }

        public DisplayAttribute(bool display)
        {
            this.display = display;
        }
    }


    public enum EnumAppendItemType
    {
        None,
        //
        // 摘要:
        //     默认“所有”项
        [Description("--所有--")]
        All,
        //
        // 摘要:
        //     默认“请选择”项
        [Description("--请选择--")]
        Select
    }
}
