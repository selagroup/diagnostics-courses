using SDPApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SDPApp.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var speakers = await ConferenceService.GetSpeakers();
            return View(speakers);
        }

        public async Task<ActionResult> SpeakerDetails(string speakerName)
        {
            if (speakerName.Contains("tein"))
            {
                return View("Error");
            }

            var speaker = await ConferenceService.GetSpeakerByName(speakerName);
            return View(speaker);
        }
    }
}