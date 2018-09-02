using System;

namespace lbucket
{
    public class MinimalisticTokenBucket
    {
        private readonly long capacity;
        private readonly double refillTokensPerOneMillis;

        private double availableTokens;
        private long lastRefillTimestamp;

        private object objLock=new object();

        public MinimalisticTokenBucket(long capacity, long refillTokens, long refillPeriodMillis)
        {
            this.capacity=capacity;

            refillTokensPerOneMillis=refillTokens/(double)TimeSpan.FromMilliseconds(refillPeriodMillis).Ticks;

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
                double refill=sinceLastTicks*refillTokensPerOneMillis;
                
                availableTokens=Math.Min(capacity,availableTokens+refill);
                //Console.WriteLine($"availableTokens:{availableTokens} refill:{refill} sinceLastTicks:{sinceLastTicks} refillTokensPerOneMillis:{refillTokensPerOneMillis}");

                lastRefillTimestamp=currentTicks;
            }
        }

    }
}