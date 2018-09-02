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