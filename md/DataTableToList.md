# DataTable To List<T>

```
public static class Extensions
    {
        public static IEnumerable<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            IList<T> list = new List<T>();
            if (dt == null || dt.Rows.Count==0)
                return list;

            PropertyInfo[] typeProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string[] arrColumns = new string[dt.Columns.Count];
            for(int i=0;i< dt.Columns.Count;i++)
            {
                arrColumns[i] = dt.Columns[i].ColumnName.ToLower();
            }

            foreach (var m in dt.Rows)
            {
                var dr2 = (DataRow)m;
                
                var columns = dt.Columns.Count;
                foreach (var p in typeProps)
                {
                    var t = new T();
                    if (arrColumns.Contains(p.Name.ToLower()))
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


#### DataTable To List<T> Run:10000
1. Benchmark 1 Reflection ms:289
2. Benchmark 2 set ms:233
3. Benchmark 3 Expressions ms:319
4. Benchmark 4 Emit ms:33
    
