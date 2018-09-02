using System;
using System.Threading;

namespace lbucket
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //Leaky_Bucket();
            Token_Bucket();
            
        }

        static void Token_Bucket()
        {
            var tb=new MinimalisticTokenBucket(4,1,1000);

            int i=0;
            long currentTicks=DateTime.Now.Ticks;
            int consumed=0;
            while(DateTime.Now.Ticks-currentTicks<TimeSpan.FromSeconds(10).Ticks)
            {
                var flag=tb.TryConsume(1);

                if(flag)
                {
                    consumed++;
                    Console.WriteLine($"tokenbucket {i} try:1 flag:{flag}");
                }
                i++;

                Thread.Sleep(DateTime.Now.Millisecond%200);
            }

            Console.WriteLine($"i:{i},consumed:{consumed}");
        }

        static void Leaky_Bucket()
        {
            var lb=new LeakyBucket(4,2000);

            for(var i=0;i<1000;i++)
            {
                var flag=lb.tryConsume(1);
                if(flag)
                    Console.WriteLine($"{i} try:1 flag:{flag}");

                Thread.Sleep(DateTime.Now.Millisecond%200);
            }
        }
    }
}
