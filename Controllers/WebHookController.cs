using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace website_ci.Controllers
{
    [Route("[controller]")]
    public class WebHookController : Controller
    {
        [HttpPost]
        public async Task<bool> HandleHook([FromBody]JObject o)
        {
            string file = string.Empty;
            using (StreamReader reader = new StreamReader(System.IO.File.Open("settings.json", FileMode.Open)))
            {
                file = reader.ReadToEnd();
            }
            JObject settings = JObject.Parse(file);
            if (Request.Headers["X-GitHub-Event"] == "push")
            {
                string repo = (string)o["repository"]["name"];
                if (settings[repo] != null)
                {
                    await SendSlackMessage($"website-ci started update of {repo}");
                    foreach(JObject command in (JArray)settings[repo]["commands"])
                    {
                        string processCommand = (string)command["process"];
                        string argsString = (string)command["args"];
                        ProcessStartInfo info = new ProcessStartInfo(processCommand, argsString);
                        info.UseShellExecute = false;
                        info.WorkingDirectory = (string)settings[repo]["workingDir"];
                        await SendSlackMessage($"Running: {processCommand} {argsString}");
                        Process process = Process.Start(info);
                        process.WaitForExit();
                        if (process.ExitCode != 0)
                        {
                            await SendSlackMessage($"{repo} command {processCommand} {argsString} exited with code {process.ExitCode}");
                        }
                    }
                    await SendSlackMessage($"website-ci finished update of {repo}");
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public async Task SendSlackMessage(string message)
        {
            if (string.IsNullOrEmpty(Secrets.SlackBotToken) || string.IsNullOrEmpty(Secrets.SlackUserId))
            {
                return;
            }
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys.Add("token", Secrets.SlackBotToken);
            keys.Add("channel", Secrets.SlackUserId);
            keys.Add("text", message);
            keys.Add("username", "website-ci");
            HttpClient client = new HttpClient();
            FormUrlEncodedContent content = new FormUrlEncodedContent(keys);
            HttpResponseMessage response = await client.PostAsync("https://slack.com/api/chat.postMessage", content);
        }
    }
}
