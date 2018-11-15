### 前言 

最新一直在忙着项目上的事情，很久没有写博客了，在这里对关注我的粉丝们说声抱歉，后面我可能更多的分享我们在微服务落地的过程中的一些经验。

那么今天给大家讲一下在 .NET Core 2 中引入的全新 DiagnosticSource 事件机制，为什么说是全新呢？ 在以前的 .NET Framework 有心的同学应该知道也有 Diagnostics，

那么新的 .NET Core 中有什么变化呢？ 让我们一起来看看吧。 

### Diagnostics Diagnostics 

一直是一个被大多数开发者忽视的东西，我猜测很多同学看到这里的时候可能还是第一次听说 Diagnostics 这个东西，为什么会被忽视呢？ 我们等会说，我们先来看一下 Diagnostics 是用来做什么的。 Diagnostics 是什么呢？ 让我们把时间往前拉回到 2013 年 8 月，微软在 NuGet 发布了一个新的关于 Diagnostics 的包叫做 [`Microsoft.Diagnostics.Tracing.TraceEvent`](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent)，这个包用来为 Windows 事件追踪（ETW）提供一个强大的支持，使用这个包可以很容易的为我们在云环境和生产环境来提供端到端的监控日志事件记录，它轻量级，高效，并且可以和系统日志进行交互。 

>PS：通过这个包我们可以获取到 CLR 运行的一些细节信息，由于本篇主题，对此不介绍过多了。

 看到这个包提供的功能，那么博主就自己总结一下，对 Diagnostics 下个定义 ：在应用程序出现问题的时候，特别是出现可用性或者性能问题的时候，开发人员或者IT人员经常会对这些问题花费大量的时间来进行诊断，很多时候生产环境的问题都无法复现，这可能会对业务造成很大的影响，Diagnostics 就是提供一组功能使我们能够很方便的可以记录在应用程序运行期间发生的关键性操作以及他们的执行时间等，使管理员可以查找特别是生产环境中出现问题所在的根本原因。 
 
 有同学可能会说了，这不就是 APM(Application Performance Management) 么，嗯，从宏观的角度来说这属于APM的一部分，但 APM 不仅仅只有这些。 
 
 ### .NET Framework 之 EventSource 
 
 在上面我们了解到了 `Microsoft.Diagnostics.Tracing.TraceEvent`，那么相关搭配使用的还有两个 NuGet 包就是 `Microsoft.Diagnostics.Tracing.EventSource` 这个包，那我就简单讲一下，我不准备在这个部分讲述太多，毕竟已经被替换掉了，我们来看下 EventSource。 
 
 #### EventSource 
 在 .NET Framework 中 EventSource 通过 Windows ETW 提供的 ETW Channels 与其进行集成，下面给出一个示例代码： 
 
 ```
 cs 
 [EventSource(Name = "Samples-EventSourceDemos-Minimal")] 
 public sealed class MinimalEventSource : EventSource 
 { 
     // Define singleton instance 
     public static MinimalEventSource Log = new MinimalEventSource(); 
     // Define Event methods 
     public void Load(long baseAddress, string imageName) 
     { 
         WriteEvent(1, baseAddress, imageName); 
    } 
 } 

 ``` 
 
 那么在 ETW 中我们就可以看到相关的事件信息了： ![img](https://images2018.cnblogs.com/blog/250417/201804/250417-20180413110235765-1125727668.png) 
 
 > 注意，在 .NET Framework 4.5 以及更高版本，`EventSource` 已经被集成到了 System 命名空间。 学习，也是一个总结的过程，对此，我们也许可以总结出来一个比较重要的信息就是：
 通过 Diagnostics 的命名空间变化，由 Microsoft 变为了 System， 我们可以看到 Diagnostics 对于我们的应用程序来说变得更加重要了。 
 由于 EventSource 只支持 Windows，所以在全新的 .NET Core 中，它已经被悄悄的取代了，下面我们来看一下全新的 DiagnosticSource。 
 
 ### .NET Core 之 全新 DiagnosticSource 
 
 在 .NET Core 中 .NET 团队设计了一个全新的 `DiagnosticSource`，新的 `DiagnosticSource` 非常的简单，它允许你在生产环境记录丰富的 payload 数据，然后你可以在另外一个消费者可以消费感兴趣的记录，是不是听着有点懵逼？没关系，等会我再详细说。 我们先来说说 `DiagnosticSource` 和上面的 `EventSource` 的区别，他们的架构设计有点类似，主要区别是 `EventSource` 它记录的数据是可序列化的数据，会被在进程外消费，所以要求记录的对象必须是可以被序列化的。而 `DiagnosticSource` 被设计为在进程内处理数据，所以通过它可以拿到更加丰富的一些数据信息，它支持非序列化的对象，比如 `HttpContext` , `HttpResponseMessage` 等。如果你想在 `EventSource` 中获取 `DiagnosticSource` 中的事件数据，你可以通过 `DiagnosticSourceEventSource` 这个对象来进行数据桥接。 下面我们来看一下在代码中如何使用 `DiagnosticSource`对象。 
 
 在这之前我们需要了解另外一个对象 **DiagnosticListener**，`DiagnosticListener` 从命名上来看它是一个监听诊断信息的对象，它确实是一个用来接收事件的类，在 .NET Core 中 **DiagnosticSource** 它其实是一个抽象类，定义了记录事件日志所需要的方法，那么我们在使用的时候就需要使用具体的对象，`DiagnosticListener` 就是 `DiagnosticSource` 的默认实现，明白了吧。 好了，现在我们来看一下如何使用吧。 
 
 #### 生成 Diagnostic 日志记录 
 
 如何生成 Diagnostic 日志记录呢？首先，我们需要创建一个 `DiagnosticListener` 对象，比如： 
 ```
 cs 
 private static DiagnosticSource httpLogger = new DiagnosticListener("System.Net.Http"); 
 ``` 
 `DiagnosticListener` 参数中的名称即为需要监听的事件（组件）名称，这个名称在以后会被用来被它的消费者所订阅使用。 `DiagnosticSource` 其核心只包含了两个方法，分别是 ： 
 ```
  bool IsEnabled(string name) 
  void Write(string name, object value); 

``` 
那么然后我们可以这样来调用： 
``` 
if (httpLogger.IsEnabled("RequestStart"))
{ 
    httpLogger.Write("RequestStart", new { Url="http://clr", Request=aRequest }); 
} 

``` 
`IsEnabled(string param1)` 这个方法用来判断是否有消费者注册了当前的事件（组件）名称监听，通常有消费者关心了相关数据，我们才会进行事件记录。 `Write(string param1,object param2)` 这个方法用来向 DiagnosticSource 中写入日志记录，`param1` 和上面一样用来指定名称的，也就是所向指定名称中写入数据，`param2` 即为写入的 payloads 数据，你可以使用 匿名类型来向 `param2` 中写入数据，这样会方便很多。 这样，我们就已经把 Diagnostic 事件日志写入到 DiagnosticSource中了，是不是很简单？ 我们再看一下如何进行消费（监听）这些事件信息。 

#### 监听 Diagnostic 日志记录 
在监听 Diagnostic 日志记录之前你需要知道你要关心的事件数据名称，那么如果仅仅是在代码中把 `DiagnosticListeners` 都写死到监听的消费者代码中的话，这样就太不灵活了，所以这里设计了一个机制用来发现中那些在运行时被激活的`DiagnosticListeners`。 你可以使用 `DiagnosticListener.AllListeners` 来获取一个 `IObservable`对象，`IObservable`接口大家应该都不陌生了吧（不太清楚的可以看[这里](https://msdn.microsoft.com/library/hh242985.aspx)），然后通过其`Subscribe`方法进行OnNext“回调”关心的事件数据。 示例代码： 
```
cs 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;

class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var log= new DiagnosticListener("System.Net.Http");
            new DiagnosticManage().Init();

            if (log.IsEnabled("RequestStart"))
            {
                log.Write("RequestStart", new { Url = "http://clr", Request = "haaaaa" });
            }

            var log2 = new DiagnosticListener("Temp");
            if (log2.IsEnabled("TempStart"))
            {
                log2.Write("TempStart", new { Url = "http://clr", Request = "haaaaa" });
            }

        }
    }

    public class DiagnosticManage
    {
        static object objLock = new object();
        static IDisposable networkSubscription = null;
        public void Init()
        {
            
            var listenerSubscription = DiagnosticListener.AllListeners.Subscribe((DiagnosticListener listener)=>{
                if (listener.Name == "System.Net.Http")
                {
                    // 订阅者监听消费代码 
                    lock (objLock)
                    {
                        if (networkSubscription != null)
                            networkSubscription.Dispose(); //回调业务代码 

                        Action<KeyValuePair<string,object>> callback =(evnt) => {

                            Console.WriteLine("From Listener {0} Received Event {1} with payload {2}", listener.Name, evnt.Key, evnt.Value);

                        };

                        //创建一个匿名Observer对象 
                        IObserver<KeyValuePair<string, object>> observer = new AnonymousObserver<KeyValuePair<string, object>>(callback); //筛选你感兴趣的事件 
                        Predicate<string> predicate = (string eventName) => eventName == "RequestStart";

                        networkSubscription = listener.Subscribe(observer, predicate);
                    }
                }
                else
                {
                    Action<KeyValuePair<string, object>> callback = (evnt) => {

                        Console.WriteLine("From Listener {0} Received Event {1} with payload {2}", listener.Name, evnt.Key, evnt.Value);

                    };

                    //创建一个匿名Observer对象 
                    IObserver<KeyValuePair<string, object>> observer = new AnonymousObserver<KeyValuePair<string, object>>(callback); //筛选你感兴趣的事件 
                    Predicate<string> predicate = (string eventName) => eventName == "TempStart";

                    listener.Subscribe(observer, predicate);
                }
            });
        }
    }


``` 

通过这种方式，我们就可以在触发回调的之后做一些我们想要的操作了。 是不是发现上面的那种写法有点麻烦和丑陋，ASP.NET 团队考虑到了，所以为我们封装了一个适配器的库来方便我们进行监听的一些操作，你可以通过打 attribute 标记的方式来进行相关事件的订阅，有兴趣的同学可以看下这个（`Microsoft.Extensions.DiagnosticAdapte`） NuGet 包。 

现在我们已经可以拿到数据了，有同学可能会说在生产环境数据这么多，这些数据我存到哪里，又怎么样来处理呢，我不可能一条一条的来找性能在哪里吧，OK，我们接着往下看。 

### 为你的框架支持 Diagnostics 
随着微服务的流行，服务的链路追踪以及应用程序的性能问题变得越来越重要，而 APM 也成为了整个微服务架构中很重要的一个中间件，它可以协助我们快速查找生产环境中所遇到的问题，以及在应用程序发生异常的时候收集异常运行时的上下文信息来快速排查问题。

 对 Google 的 Dapper 或者 OpenTracing 协议有了解的同学应该已经想到了，我们可以利用上面的那些数据按照这些协议的约定进行包装，然后发送到支持这些协议的 APM 的服务端，剩下的工作是不是可以由这些服务端来帮助我们处理了，包括图形化展示，性能查看，调用链查看等。 大多数的开源APM项目都支持 Dapper 或者 OpenTracing 协议，如 [Apache SkyWalking](https://github.com/apache/incubator-skywalking) , [ZipKin](https://github.com/openzipkin/zipkin)，[pinpoint](https://github.com/naver/pinpoint) 等。 顺便说一句，我们 [NCC开源项目组](https://github.com/dotnetcore) 的 Lemon 同学正在给 SkyWalking 写 C# 的 [客户端驱动项目](https://github.com/OpenSkywalking/skywalking-csharp) ，这是一项非常具有挑战性的工作，感兴趣的同学可以 Star 一下。 相信阅读本篇文章也有不少的架构师，开源项目作者，框架开发者，甚至应用程序开发者，那么我建议可以从现在开始对你的项目提供 Diagnostics 支持，目前 .NET Core 中 CoreFx , ASP.NET Core, EntityFramework Core 都已经对 Diagnostics 提供了支持。 CAP 在 2.2 版本中已经对 Diagnostics 提供了支持。 
 ### CAP 中的 Diagnostics 
 CAP: [https://github.com/dotnetcore/CAP](https://github.com/dotnetcore/CAP) CAP 是我的一个开源项目，用来处理在微服务或者SOA架构中分布式事务的一个解决方案，你可以在[这篇文章](http://www.cnblogs.com/savorboard/p/cap.html)中看到更多关于 CAP 的介绍，喜欢的同学可以给个 Star ，也是我继续做的更好的动力，谢谢。 CAP 对外提供的事件监听者名称为： `CapDiagnosticListener` CAP 中的 Diagnostics 提供对外提供的事件信息有：
  * 消息持久化之前 
  * 消息持久化之后 
  * 消息持久化异常 
  * 消息向MQ发送之前 
  * 消息向MQ发送之后 
  * 消息向MQ发送异常 
  * 消息从MQ消费保存之前 
  * 消息从MQ消费保存之后 
  * 订阅者方法执行之前 
  * 订阅者方法执行之后 
  * 订阅者方法执行异常 
  
  相关涉及到的对象，你可以在 `DotNetCore.CAP.Diagnostics` 命名空间下看到。 基于这些对外的事件数据，我们可以来对接APM，下面这个是我对接的 ZipKin 的一个图： ![](https://images2018.cnblogs.com/blog/250417/201804/250417-20180413165322008-1243832895.png) 
  
  ### 总结 
  
  通过本篇文章我们知道了 .NET Core 中为我们提供的一个新的事件数据记录对象DiagnosticSource ，通过这个对象，我们可以对外提供一些诊断信息，以便于在生产环境中对我们的应用程序进行性能问题排查和调用链跟踪，然后我们知道了一下CAP对外提供的一些Diagnostics事件。 如果你觉得本篇文章对您有帮助的话，感谢您的【推荐】。 如果你对 .NET Core 有兴趣的话可以关注我，我会定期的在博客分享我的学习心得。 --- >本文地址：[http://www.cnblogs.com/savorboard/p/diagnostics.html](http://www.cnblogs.com/savorboard/p/diagnostics.html) >作者博客：[Savorboard](http://www.cnblogs.com/savorboard) >本文原创授权为：署名 - 非商业性使用 - 禁止演绎，协议[普通文本](https://creativecommons.org/licenses/by-nc-nd/4.0/) | 协议[法律文本](https://creativecommons.org/licenses/by-nc-nd/4.0/legalcode)
  
  引用:https://www.cnblogs.com/savorboard/p/diagnostics.html
