using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLeak
{
    class Response
    {
        private Request _request;
        private DbConnection _dbConnection;
        private byte[] _data;

        public Response(Request request)
        {
            _request = request;
            ResponseCache.Add(this);
        }

        public void Send()
        {
            _dbConnection = new DbConnection();
            _dbConnection.Open();
            _data = _dbConnection.GenerateResponse(_request.Id);
        }
    }
}
