using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SDPApp.Shared
{
    public class ConferenceService
    {
        const string CDN_BASE_URL = "http://sdp2.blob.core.windows.net";

        private readonly string _appDataPath;
        private Dictionary<string, Speaker> _speakers;
        private Dictionary<string, Session> _sessions;

        public ConferenceService(string appDataPath)
        {
            _appDataPath = appDataPath;
        }

        public async Task<Speaker> GetSpeakerByName(string name)
        {
            if (_speakers == null)
                await GetSpeakers();
            
            var speaker = _speakers[name];
            return speaker;
        }

        public async Task<byte[]> GetSpeakerPhoto(string speakerName)
        {
            // A stub to generate a large object that will leak.
            return await Task.Run(() => new byte[5000000]);
        }

        public async Task<Session> GetSessionById(string id)
        {
            if (_sessions == null)
                await GetSpeakers();
            return _sessions[id];
        }

        private async Task<string> ReadAllTextAsync(string filename)
        {
            using (FileStream fs = File.OpenRead(Path.Combine(_appDataPath, filename)))
            {
                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, (int)fs.Length);
                return Encoding.UTF8.GetString(buffer);
            }
        }

        public async Task<IEnumerable<Speaker>> GetSpeakers()
        {
            if (_speakers != null)
                return _speakers.Values;

            _speakers = new Dictionary<string, Speaker>();
            _sessions = new Dictionary<string, Session>();

            List<Speaker> speakers = new List<Speaker>();

            JObject speakersJson = JObject.Parse(await ReadAllTextAsync("speakers.json"));
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

            JObject sessionsJson = JObject.Parse(await ReadAllTextAsync("sessions.json"));
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
                        if (!_speakers.ContainsKey(speakerName))
                            continue;
                        Session newSession = new Session
                        {
                            Title = session.title,
                            Description = session.@abstract,
                            Track = trackTitles[(int)session.track.Value],
                            Time = session.time,
                            Id = Guid.NewGuid().ToString()
                        };
                        _speakers[speakerName].Sessions.Add(newSession);
                        _sessions.Add(newSession.Id, newSession);
                    }
                }
            }

            return speakers;
        }
    }
}
