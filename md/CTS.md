### 什么是CTS？

当你需要设计面向.Net的语言时所需要遵循一个体系(.Net平台下的语言都支持的一个体系)这个体系就是CTS（Common Type System 公共类型系统），它包括但不限于：

建立用于跨语言执行的框架。

- 提供面向对象的模型，支持在 .NET 实现上实现各种语言。
- 定义处理类型时所有语言都必须遵守的一组规则(CLS)。
- 提供包含应用程序开发中使用的基本基元数据类型（如 Boolean、Byte、Char 等）的库。

**CLS**是CTS（Common Type System 公共类型系统）这个体系中的子集。

一个编程语言，如果它能够支持CTS，那么我们就称它为面向.NET平台的语言。

官方CTS介绍： https://docs.microsoft.com/zh-cn/dotnet/standard/common-type-system 

微软已经将CTS和.NET的一些其它组件，提交给ECMA以成为公开的标准，最后形成的标准称为CLI（Common Language Infrastructure）公共语言基础结构。

所以有的时候你见到的书籍或文章有的只提起CTS，有的只提起CLI，请不要奇怪，你可以宽泛的把他们理解成一个意思，CLI是微软将CTS等内容提交给国际组织计算机制造联合会ECMA的一个工业标准。
