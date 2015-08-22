using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLeak
{
    class ResponseCache
    {
        private static List<Response> _cache = new List<Response>();

        public static void Add(Response response)
        {
            _cache.Add(response);
        }
    }
}
