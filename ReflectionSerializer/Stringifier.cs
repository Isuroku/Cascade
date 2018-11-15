using System;
using System.Text;

namespace CascadeSerializer
{
    public static class Stringifier
    {
        public static string Stringify(this SerializedObject instance)
        {
            return Stringify(instance, string.Empty);
        }
        static string Stringify(SerializedObject instance, string indent)
        {
            return Stringify(instance, indent, false);
        }

        static string Stringify(SerializedObject instance, string indent, bool skipFirst)
        {
            if (instance is SerializedAtom) return Stringify(instance as SerializedAtom, indent, skipFirst);
            if (instance is SerializedAggregate) return Stringify(instance as SerializedAggregate, indent, skipFirst);
            if (instance is SerializedCollection) return Stringify(instance as SerializedCollection, indent, skipFirst);
            throw new InvalidOperationException();
        }

        static string Stringify(SerializedCollection instance, string indent, bool skipFirst)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0}{1}{2}", NameString(instance, indent, skipFirst), TypeString(instance), Environment.NewLine);
            builder.AppendLine(indent + "{");
            foreach (var item in instance.Items)
            {
                if (item is SerializedAtom)
                    builder.AppendLine(Stringify(item as SerializedAtom, indent + "  "));
                else
                    builder.Append(Stringify(item, indent + "  "));
            }
            builder.AppendLine(indent + "}");

            return builder.ToString();
        }

        static string Stringify(SerializedAggregate instance, string indent, bool skipFirst)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0}{1}{2}", NameString(instance, indent, skipFirst), TypeString(instance), Environment.NewLine);
            builder.AppendLine(indent + "{");
            foreach (var key in instance.Children.Keys)
            {
                if (key is SerializedAtom)
                    builder.Append(Stringify(key as SerializedAtom, indent + "  ") + " ");
                else if (key is SerializedObject)
                {
                    builder.Append(Stringify(key as SerializedObject, indent + "  "));
                    builder.Append(indent + "    ");
                }
                else
                    builder.Append(indent + "  " + key + " ");

                builder.Append("-> ");

                if (instance.Children[key] is SerializedAtom)
                    builder.AppendLine(Stringify(instance.Children[key] as SerializedAtom, indent + "  ", true));
                else
                    builder.Append(Stringify(instance.Children[key], indent + "  ", true));
            }
            builder.AppendLine(indent + "}");

            return builder.ToString();
        }

        static string Stringify(SerializedAtom instance, string indent, bool skipFirst)
        {
            string valueString = instance.Value == null ? "(null)" : instance.Value.ToString();
            return string.Format("{0}{1} = {2}",
                NameString(instance, indent, skipFirst), TypeString(instance), valueString);
        }

        static string NameString(SerializedObject instance, string indent, bool skip)
        {
            return (skip ? string.Empty : indent) + (instance.Name == null ? "(unnamed)" : "'" + instance.Name + "'");
        }
        static string TypeString(SerializedObject instance)
        {
            return instance.Type == null ? string.Empty : " :: " + instance.Type;
        }
    }
}
