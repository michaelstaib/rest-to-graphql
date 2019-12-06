using System.Net;
using System.Net.Http;
using HotChocolate.Types;
using Newtonsoft.Json;

namespace RestToGraphQL
{
    public class HttpGetDirectiveType : DirectiveType<HttpGetDirective>
    {
        protected override void Configure(IDirectiveTypeDescriptor<HttpGetDirective> descriptor)
        {
            descriptor.Name("httpGet");
            descriptor.Location(HotChocolate.Types.DirectiveLocation.FieldDefinition);
            descriptor.Use(next => async context =>
            {
                IHttpClientFactory clientFactory = context.Service<IHttpClientFactory>();
                HttpGetDirective directive = context.Directive.ToObject<HttpGetDirective>();

                if (!context.ContextData.TryGetValue(directive.Url, out object? result))
                {
                    HttpClient client = clientFactory.CreateClient(directive.Name);
                    HttpResponseMessage message = await client.GetAsync(directive.Url);

                    if (message.StatusCode == HttpStatusCode.NotFound)
                    {
                        context.ContextData[directive.Url] = null;
                        context.Result = null;
                    }
                    else
                    {
                        message.EnsureSuccessStatusCode();

                        string json = await message.Content.ReadAsStringAsync();
                        var token = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JToken>(json);
                        context.ContextData[directive.Url] = token;
                        context.Result = token;

                        await next(context);
                    }
                }
            });
        }
    }
}
