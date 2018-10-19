# ActionFilterAttribute

## webapi 输出时Model 转 Json

#### 时间处理
1. 时间显示格式:yyyy-MM-dd HH:mm:ss
2. Long 转时间支持

```
    /// <summary>
    /// JsonIsoDateTimeConverter
    /// </summary>
    public class JsonIsoDateTimeConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public JsonIsoDateTimeConverter()
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }

        /// <summary>
        /// WriteJson
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text;

            if (value is long v)
            {
                text = new DateTime(v).ToString(DateTimeFormat, Culture);
                writer.WriteValue(text);
                return;
            }

            base.WriteJson(writer, value, serializer);
        }
    }
```

#### ContractResolver
1. Json 属性驼峰显示
2. 去掉Thrift __isset 字段
3. Thrift long类型时间字段转时间类型

```
    /// <summary>
    /// CamelCasePropertyNamesWithThriftContractResolver
    /// </summary>
    public class CamelCasePropertyNamesWithThriftContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly string[] arrTimeFlag = new string[] { "time", "timed", "date", "dated" };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var list = base.CreateProperties(type, memberSerialization);
            var n = list.Count;
            list = list.Where(m => m.PropertyName != "__isset")?.ToList();
            var isThrift = n != list.Count;

            if (isThrift)
            {
                foreach (var m in list)
                {
                    if (m.PropertyType.FullName != "System.Int64")
                        continue;

                    var propName = m.PropertyName.ToLower();
                    foreach (var str in arrTimeFlag)
                    {
                        if (propName.EndsWith(str))
                        {
                            m.Converter = new JsonIsoDateTimeConverter();
                            break;
                        }
                    }
                }
            }
            return list;
        }
    }
```

#### CustomJsonSerializerSettings

```
    /// <summary>
    /// CustomJsonSerializerSettings
    /// </summary>
    public class CustomJsonSerializerSettings : JsonSerializerSettings
    {
        /// <summary>
        /// CustomJsonSerializerSettings
        /// </summary>
        public CustomJsonSerializerSettings()
        {
            Converters.Add(new JsonIsoDateTimeConverter());
            ContractResolver = new CamelCasePropertyNamesWithThriftContractResolver();
        }
    }

```

#### Webapi ActionFilterAttribute

```
public class JsonActionFilterAttribute: System.Web.Http.Filters.ActionFilterAttribute
    {
        private static readonly JsonMediaTypeFormatter _jsonMediaType;
        static JsonActionFilterAttribute()
        {
            _jsonFormatter = new JsonMediaTypeFormatter {
                SerializerSettings=new CustomJsonSerializerSettings()
            };
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                ObjectContent newObjContent = new ObjectContent(objectContent.ObjectType, objectContent.Value, _jsonMediaType, "application/json");
                HttpResponseMessage result = new HttpResponseMessage { Content = newObjContent };
                actionExecutedContext.Response = result;
            }

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }


    }

```

#### 替换全局JsonFormatter

1. 定义 JsonContentNegotiator

```

    /// <summary>
    /// JsonContentNegotiator
    /// </summary>
    public class JsonContentNegotiator : IContentNegotiator
    {
        private static readonly JsonMediaTypeFormatter _jsonFormatter;

        static JsonContentNegotiator()
        {
            _jsonFormatter = new JsonMediaTypeFormatter {
                SerializerSettings=new CustomJsonSerializerSettings()
            };
        }

        /// <summary>
        /// Negotiate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="request"></param>
        /// <param name="formatters"></param>
        /// <returns></returns>
        public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            var result = new ContentNegotiationResult(_jsonFormatter, new MediaTypeHeaderValue("application/json"));
            return result;
        }
    }

```

2.  替换

WebApiConfig.cs WebApiConfig.Register

```
        //默认返回json格式
        config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator());
```