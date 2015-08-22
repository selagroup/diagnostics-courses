using SDPApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDPApp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var speakers = ConferenceService.GetSpeakers().Result;
        }
    }
}
