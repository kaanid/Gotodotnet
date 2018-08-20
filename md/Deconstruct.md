### Deconstruct

#### 解构元组

C# 7.0 新增一项是新元组(ValueTuple),它允许我们可以返回多个值 ，并且配合解构能更加方便的进行工作，如下面例子

```
class Program
{
    static void Main(string[] args)
    {
    (var name, var age) = GetUser();
    Console.WriteLine($"name:{name}\nage:{age}");
    }

    public static (string name,int age) GetUser()
    {
        return ("刘大神", 100);
    }
}

```

可以看到解构元组可以写出优雅的代码，并且可以使用类型推断，但在这里解构元组并不是重点，下面说一个有趣的功能。

#### 解构对象

解构能力并不是只能解构元组，他还有一个更加有意思的功能，就是解构对象，是不是有意思。

```

static  void Main(string[] args)
{
    var user=new User{
        Name="王大神",
        Age=22,
        Sex="男"
    };

    (var name,var age)=user;

    Console.WriteLine($"name:{name}\nage:{age}");
}

```

上面代码是不是很惊奇，那么这到底怎么实现的呢，其实只是在类中添加一个解构**函数(Deconstruct)**就可以，解构参数方法名称必须是Deconstruct,返回值必须是void,参数列表必须是out

```
public class User
{
    public string Name{get;set;}
    public int Age{get;set;}
    public string Sex{get;set;}

    public void Deconstruct(out string name,out int age)
    {
        name=Name;
        age=Age;
    }
}


```

只要给类加一个方法(void Deconstruct)就可以了，哈哈哈哈

解构函数还支持重载

```
class Program
{
    static void Main(string[] args)
    {
        var user=new User{
            Name="张大神",
            Age=29,
            Sex="男"
        };

        (var name,var age)=user;
        Console.WriteLine($"name:{name}\nage:{age}");

        (var name,var age,var sex)=user;
        Console.WriteLine($"name:{name}\nage:{age}\nsex:{sex}");
    }
}


public class User
{
    public string Name{get;set;}
    public int Age{get;set;}
    public string Sex{get;set;}

    public void Deconstruct(out string name,out int age)
    {
        name=Name;
        age=Age;
    }

    public void Deconstruct(out string name,out int age,out string sex)
    {
        name=Name;
        age=Age;
        sex=Sex;
    }
}

```

但是解构不支持参数一致的重载

```
public class User
{
    public string Name{get;set;}
    public int Age{get;set;}
    public string Sex{get;set;}
    public string Title{get;set;}

    public void Deconstruct(out string name,out string Title)
    {
        name=Name;
        age=Age;
    }

    public void Deconstruct(out string name,out string sex)
    {
        name=Name;
        age=Age;
        sex=Sex;
    }
}
```
哪怕参数类型不一致
```
public class User
{
    public string Name{get;set;}
    public int Age{get;set;}
    public string Sex{get;set;}
    public string Title{get;set;}

    public void Deconstruct(out string name,out int age)
    {
        name=Name;
        age=Age;
    }

    public void Deconstruct(out string name,out string sex)
    {
        name=Name;
        age=Age;
        sex=Sex;
    }
}

```

VS 会提示错误

感觉像参数类型推断错误，但是VS提示“找不到类型适用的Deconstruct”

所以解构函数并不支持参数数量相同的重载，哪怕参数类型不一致