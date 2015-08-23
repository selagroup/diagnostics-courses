using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDPApp.Shared
{
    public class Speaker
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string PhotoURL { get; set; }
        public string Blog { get; set; }
        public string Twitter { get; set; }
        public ICollection<Session> Sessions { get; private set; }

        public Speaker()
        {
            Sessions = new List<Session>();
        }
    }
}
