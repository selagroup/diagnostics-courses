using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SDPApp.Shared
{
    public class ConferenceService
    {
        const string SPEAKERS_URL = "http://www.seladeveloperpractice.com/api/speakers";
        const string SESSIONS_URL = "http://www.seladeveloperpractice.com/api/sessions";
        const string CDN_BASE_URL = "http://sdp2.blob.core.windows.net";

        private static Dictionary<string, Speaker> _speakers;
        private static Dictionary<string, Session> _sessions;

        private static Random _rand = new Random();

        public static async Task<Speaker> GetSpeakerByName(string name)
        {
            if (_speakers == null)
                await GetSpeakers();
            return _speakers[name];
        }

        public static async Task<Session> GetSessionById(string id)
        {
            if (_sessions == null)
                await GetSpeakers();
            return _sessions[id];
        }

        public static async Task<IEnumerable<Speaker>> GetSpeakers()
        {
            if (_rand.Next() % 10 == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            if (_speakers != null)
                return _speakers.Values;

            _speakers = new Dictionary<string, Speaker>();
            _sessions = new Dictionary<string, Session>();

            List<Speaker> speakers = new List<Speaker>();

            HttpClient http = new HttpClient();
            JObject speakersJson = JObject.Parse(await http.GetStringAsync(SPEAKERS_URL));
            foreach (var item in speakersJson)
            {
                dynamic value = item.Value;
                Speaker speaker = new Speaker
                {
                    Name = item.Key,
                    Bio = value.bio,
                    Blog = value.blog,
                    Twitter = value.twitter,
                    PhotoURL = CDN_BASE_URL + value.photo
                };
                speakers.Add(speaker);
                _speakers.Add(speaker.Name, speaker);
            }

            JObject sessionsJson = JObject.Parse(await http.GetStringAsync(SESSIONS_URL));
            foreach (var day in sessionsJson)
            {
                Dictionary<int, string> trackTitles = new Dictionary<int, string>();
                dynamic value = day.Value;
                dynamic tracks = value.tracks;
                for (int i = 0; i < tracks.Count; ++i)
                {
                    dynamic track = tracks[i];
                    trackTitles.Add((int)track.index.Value, (string)track.title);
                }
                dynamic slots = value.slots;
                foreach (var slot in slots)
                {
                    dynamic sessions = slot.Value.sessions;
                    for (int i = 0; i < sessions.Count; ++i)
                    {
                        dynamic session = sessions[i];
                        string speakerName = session.speaker;
                        Session newSession = new Session
                        {
                            Title = session.title,
                            Description = session.@abstract,
                            Track = trackTitles[(int)session.track.Value],
                            Time = session.time,
                            Id = Guid.NewGuid().ToString()
                        };
                        if (speakerName.ToLower() != "tbd")
                        {
                            _speakers[speakerName].Sessions.Add(newSession);
                        }
                        _sessions.Add(newSession.Id, newSession);
                    }
                }
            }

            return speakers;
        }
    }
}
