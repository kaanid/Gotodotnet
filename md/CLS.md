#CLS

### 什么是CLS

这几年编程语言层出不穷，在将来.NET可能还会支持更多的语言，如果说对一个开发者而言掌握所有语言的差异处这是不现实的，所以.NET专门为此参考每种语言并找出了语言间的共性，然后定义了一组规则，开发者都遵守这个规则来编码，那么代码就能被任意.NET平台支持的语言所通用。而与其说是规则，不如说它是一组语言互操作的标准规范，它就是公共语言规范 - Common Language Specification ,简称CLS

在面向.NET开发中，编写跨语言组件时所遵循的那些共性，那些规范就叫做 Common Langrage Specification简称 CLS，公共语言规范 

官方CLS介绍：https://docs.microsoft.com/zh-cn/dotnet/standard/language-independence-and-language-independent-components

CLS从类型、命名、事件、属性、数组等方面对语言进行了共性的定义及规范。这些东西被提交给欧洲计算机制造联合会ECMA，称为：共同语言基础设施。

当然，就编码角度而言，我们不是必须要看那些详略的文档。为了方便开发者开发，.NET提供了一个特性，名叫：CLSCompliantAttribute，代码被CLSCompliantAttribute标记后，如果你写的代码不符合CLS规范的话，编译器就会给你一条警告。

### CLS 类型

#### C#语言符合规范类型

Byte 8位无符号整数

Int16 16位带符号整数

Int32 32位带符号整数

Int64 64位带符号整数

Single 单精度浮点值

Double 双精度浮点值

Boolean true 或 false值类型

Char UTF16编码单元

Decimal 非浮点十进制数字

IntPtr 平台定义的大小的指针或包柄

String 包含零个、一个或多个Char对象的集合

#### C#语言不符合规范类型

SByte 8位带符号整数数据类型，符合CLS的替代类型:Int16

TypedReference 指向对象及其运时类型的指针

UInt16 16位无符号整数，符合CLS的替代类型:Int32

Uint32 32位无符号整数，符合CLS的替代类型:Int64

Uint64 64位无符号整数，符合CLS的替代类型:Int64(可能溢出)|BigInteger|Double

UinPtr 未签名的指针或句柄，符合CLS的替代类型:IntPtr

### CLS异常

提到特殊情况，还要说的一点就是异常处理。.NET框架组成中定义了异常类型系统，在编译器角度，所有catch捕获的异常都必须继承自System.Exception，如果你要调用一个 由不遵循此规范的语言 抛出其它类型的异常对象(C++允许抛出任何类型的异常，如C#调用C++代码，C++抛出一个string类型的异常)，在C#2.0之前Catch(Exception)是捕捉不了的，但之后的版本可以。

在后续版本中，微软提供了System.Runtime.CompilerServices.RuntimeWrappedException异常类，将那些不符合CLS的包含Exception的对象封装起来。并且可以通过RuntimeCompatibilityAttribute特性来过滤这些异常。

RuntimeWrappedException ：https://docs.microsoft.com/zh-cn/dotnet/api/system.runtime.compilerservices.runtimewrappedexception?view=netframework-4.7.2
