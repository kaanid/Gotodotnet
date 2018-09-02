# 限流

现实场景，商场推出100件特价商品，早上9点准时开卖。工作人员预估有会有10000人来抢购。
但工作人员有限，特价商口只分配2个工作人员，最多同时接受2个客户购买。为了避免现场混乱，商场想要必须控制流量。

1. 只设置2个出入口
2. 按排队顺序，每个门每次进1名顾客进入商场进行购买
3. 顾客购买或放弃后，再放下一批顾客进入商场进行购买
4. 直到商品卖完为止

互联网应用，秒杀商品场景同样如此，WEB服务器一秒内要承受平时100倍流量，为了保证WEB服务不受影响，必须做限流处理。

1. 分布式WEB服务，承接流量
2. 每个入口一段时间入只能进入N个流量
3. 进入流量，程序处理下单，或放弃
4. 再放N个流量进入，步骤3
5. 直到商品卖完为止

其实不管处理何种场景，本质都是降低流量保证应用的高可用。


## 漏桶限流法

漏桶算法比较简单，就是将流量放入桶中，漏桶同时也按照一定的速率流出，如果流量过快的话就会溢出(漏桶并不会提高流出速率)。溢出的流量则直接丢弃。

![img](https://i.loli.net/2017/08/11/598c905caa8cb.png)

这种做法简单粗暴。

漏桶算法虽说简单，但却不能应对实际场景，比如突然暴增的流量。

```
using System;

namespace lbucket
{
    public class LeakyBucket {

        private readonly long capacity;
        private readonly long leaksIntervalInTicks;

        private object objLock=new object();

        private double used;
        private long lastLeakTimestamp;

        public LeakyBucket(long capacity, long leaksIntervalInMillis) 
        {
            this.capacity = capacity;
            this.leaksIntervalInTicks =TimeSpan.FromMilliseconds(leaksIntervalInMillis).Ticks;

            this.used = 0;
            this.lastLeakTimestamp = DateTime.Now.Ticks;
        }

        private long GetTotalMillis()
        {
            TimeSpan ts=new TimeSpan(DateTime.Now.Ticks);
            return (long)ts.TotalMilliseconds;
        }

        public bool tryConsume(int drop) 
        {

            lock(objLock)
            {
                leak();

                if (used + drop > capacity) 
                {
                    return false;
                }

                used = used + drop;
                return true;
            }
        }

        private void leak() 
        {
            long currentTimeTicks = DateTime.Now.Ticks;
            if (currentTimeTicks > lastLeakTimestamp) 
            {
                long millisSinceLastLeak = currentTimeTicks - lastLeakTimestamp;
                long leaks = millisSinceLastLeak / leaksIntervalInTicks;
                if(leaks > 0)
                {
                    if(used <= leaks){
                        used = 0;
                    }else{
                        used -= (int)leaks;
                        Console.WriteLine($"used:{used} leaks:{leaks}");
                    }
                    this.lastLeakTimestamp = currentTimeTicks;
                }
            }
        }
    }

}
```

## 令牌桶限流法

令牌桶会以一个恒定的速率向固定容量大小桶中放入令牌，当有流量来时则取走一个或多个令牌。当桶中没有令牌则将当前请求丢弃或阻塞。

![img](https://i.loli.net/2017/08/11/598c91f2a33af.gif)

```
using System;

namespace lbucket
{
    public class MinimalisticTokenBucket
    {
        private readonly long capacity;
        private readonly double refillTokensPerOneTicks;

        private double availableTokens;
        private long lastRefillTimestamp;

        private object objLock=new object();

        public MinimalisticTokenBucket(long capacity, long refillTokens, long refillPeriodMillis)
        {
            this.capacity=capacity;

            refillTokensPerOneTicks=refillTokens/(double)TimeSpan.FromMilliseconds(refillPeriodMillis).Ticks;

            availableTokens=capacity;

            lastRefillTimestamp=DateTime.Now.Ticks;
        }

        public bool TryConsume(int numTokens)
        {
            lock(objLock)
            {
                Refill();
                if(availableTokens<numTokens)
                    return false;

                availableTokens-=numTokens;
                return true;
            }
        }

        public void Refill()
        {
            var currentTicks=DateTime.Now.Ticks;
            if(currentTicks>lastRefillTimestamp)
            {
                var sinceLastTicks=currentTicks-lastRefillTimestamp;
                double refill=sinceLastTicks*refillTokensPerOneTicks;
                
                availableTokens=Math.Min(capacity,availableTokens+refill);
                //Console.WriteLine($"availableTokens:{availableTokens} refill:{refill} sinceLastTicks:{sinceLastTicks} refillTokensPerOneMillis:{refillTokensPerOneMillis}");

                lastRefillTimestamp=currentTicks;
            }
        }

    }
}
```

## 分布式限流

1. 使用令牌桶限流算法
2. redis 原子操作代替availableTokens

