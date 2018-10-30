# DataTable To List<T>

```
public static class Extensions
    {
        public static IEnumerable<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            IList<T> list = new List<T>();
            PropertyInfo[] typeProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in dt.Rows)
            {
                var dr2 = (DataRow)m;

                foreach (var p in typeProps)
                {
                    var t = new T();
                    if (dt.Columns.Contains(p.Name))
                    {
                        object obj = dr2[p.Name];
                        if (obj == null || obj.GetType() == typeof(DBNull))
                        {
                            continue;
                        }
                        var type = p.PropertyType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var genericType = Nullable.GetUnderlyingType(type);
                            if (genericType != null)
                            {
                                p.SetValue(t, Convert.ChangeType(obj, genericType), null);
                            }
                        }
                        else
                        {
                            p.SetValue(t, Convert.ChangeType(obj, type), null);
                        }

                        list.Add(t);
                    }
                }
            }
            return list;
        }
    }
```
