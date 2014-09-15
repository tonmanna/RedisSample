using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSample
{
    public class SampleEvent
    {
        public int ID { get; set; }
        public string EntityID { get; set; }
        public string Name { get; set; }
    }

    public static class RedisExtensions
    {
        internal static T GetFromCache<T>(this IRedisClient redisClient, string cacheKey,Func<T> factoryFn,TimeSpan expiresIn)
        {
            var res = redisClient.Get<T>(cacheKey);
            if (res != null)
            {
                redisClient.Set<T>(cacheKey, res, expiresIn);
                return res;
            }
            else
            {
                res = factoryFn();
                if (res != null) redisClient.Set<T>(cacheKey, res, expiresIn);
                return res;
            }
        }

    }


    class Program
    {
        static void Main(string[] args)
        {

            var data = Get(1);
            Console.WriteLine(data.Name);
            Console.ReadKey();
            data = Get(1);
            Console.WriteLine(data.Name);
            Console.ReadKey();

        }

        public static SampleEvent Get(int id)
        {
            using (var redisClient = new RedisClient("localhost"))
            {
                IEnumerable<SampleEvent> events = redisClient.GetFromCache<IEnumerable<SampleEvent>>("Meds25", () =>
                {

                    var medsWithID25 = new List<SampleEvent>();
                    medsWithID25.Add(new SampleEvent() { ID = 1, EntityID = "25", Name = "Digoxin" });
                    medsWithID25.Add(new SampleEvent() { ID = 2, EntityID = "25", Name = "Aspirin" });

                    return medsWithID25;

                }, TimeSpan.FromSeconds(5));

                if (events != null)
                {
                    return events.Where(m => m.ID == id).SingleOrDefault();
                }
                else
                    return null;
            }
        }
    }
}
