using System;
using System.Linq;
using System.Collections.Generic;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;

namespace RestToGraphQL
{

    public class JsonDirectiveType : DirectiveType<JsonDirective>
    {
        protected override void Configure(IDirectiveTypeDescriptor<JsonDirective> descriptor)
        {
            descriptor.Name("json");
            descriptor.Location(HotChocolate.Types.DirectiveLocation.FieldDefinition);
            descriptor.Repeatable();
            descriptor.Use(next => context =>
            {
                if (context.Result is JToken token)
                {
                    JsonDirective directive = context.Directive.ToObject<JsonDirective>();
                    JToken[] tokens = token.SelectTokens(directive.Path!).ToArray();
                    context.Result = tokens;

                    if (context.Field.Type.NamedType().IsLeafType())
                    {
                        object?[] deserialized = Deserialize(tokens);

                        if (context.Field.Type.IsListType())
                        {
                            context.Result = deserialized;
                        }
                        else
                        {
                            context.Result = deserialized[0];
                        }
                    }
                }

                return next(context);
            });
        }

        private object?[] Deserialize(JToken[] tokens)
        {
            if (tokens.Length == 0)
            {
                return Array.Empty<object>();
            }
            else if (tokens.Length == 1)
            {
                return new object?[] { Deserialize(tokens[0]) };
            }
            else
            {
                var list = new object?[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    list[i] = Deserialize(tokens[i]);
                }
                return list;
            }
        }

        private object? Deserialize(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Bytes:
                    return token.Value<byte[]>();
                case JTokenType.Date:
                    return token.Value<DateTime>();
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Guid:
                    return token.Value<Guid>();
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.TimeSpan:
                    return token.Value<TimeSpan>();
                case JTokenType.Uri:
                    return token.Value<Uri>();
                case JTokenType.Array:
                    var array = ((JArray)token);
                    var list = new object?[array.Count];
                    for (int i = 0; i < array.Count; i++)
                    {
                        list[i] = Deserialize(array[i]);
                    }
                    return list;

                case JTokenType.Null:
                default:
                    return null;
            }
        }
    }
}
