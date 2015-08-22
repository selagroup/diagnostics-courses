using SDPApp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SDPApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private ConferenceService _conferenceService;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (_conferenceService == null)
            {
                string appDataPath = Path.Combine(HttpContext.Request.PhysicalApplicationPath, "App_Data");
                _conferenceService = new ConferenceService(appDataPath);
            }
        }

        public async Task<ActionResult> Index()
        {
            var speakers = await _conferenceService.GetSpeakers();
            return View(speakers);
        }

        public async Task<ActionResult> SpeakerDetails(string speakerName)
        {
            if (speakerName.EndsWith("tein"))
            {
                var throwingTask = Task.Factory.StartNew(ThrowProhibitedSurnameException);
                // The exception is rethrown here, but the source is on another thread.
                throwingTask.Wait();
            }
            var speaker = await _conferenceService.GetSpeakerByName(speakerName);
            var speakerPhoto = await _conferenceService.GetSpeakerPhoto(speakerName);
            HttpContext.Cache.Insert(Guid.NewGuid().ToString(), speakerPhoto);
            return View(speaker);
        }
        
        private static void ThrowProhibitedSurnameException()
        {
            throw new ApplicationException("The surname suffix 'tein' is prohibited.");
        }
    }
}