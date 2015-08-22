using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLeak
{
    class Request
    {
        private Guid _id;
        private byte[] _data;

        public Guid Id { get { return _id; } }

        private Request()
        {
            _id = Guid.NewGuid();
            _data = new byte[10000];
        }

        public static Request NextRequest()
        {
            return new Request();
        }
    }
}
