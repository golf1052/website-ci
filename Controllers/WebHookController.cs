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
        public bool HandleHook([FromBody]JObject o)
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
                    _ = SendSlackMessage($"website-ci started update of {repo}");
                    JObject headCommit = (JObject)o["head_commit"];
                    if (headCommit != null)
                    {
                        _ = SendSlackMessage($"Commit message: {headCommit["message"]}");
                        JObject author = (JObject)headCommit["author"];
                        if (author != null)
                        {
                            _ = SendSlackMessage($"Committed by {author["name"]} <{author["email"]}> (https://github.com/{author["username"]})");
                        }
                    }
                    foreach(JObject command in (JArray)settings[repo]["commands"])
                    {
                        string processCommand = (string)command["process"];
                        string argsString = (string)command["args"];
                        string stringCommand = processCommand;
                        ProcessStartInfo info;
                        if (string.IsNullOrWhiteSpace(argsString))
                        {
                            info = new ProcessStartInfo(processCommand);
                        }
                        else
                        {
                            info = new ProcessStartInfo(processCommand, argsString);
                            stringCommand += $" {argsString}";
                        }
                        info.UseShellExecute = false;
                        info.RedirectStandardError = true;
                        info.WorkingDirectory = (string)settings[repo]["workingDir"];
                        _ = SendSlackMessage($"Running {stringCommand}");
                        using Process process = Process.Start(info);
                        string errorOutput = process.StandardError.ReadToEnd();
                        TimeSpan waitTime = TimeSpan.FromMinutes(5);
                        bool waitResult = process.WaitForExit((int)waitTime.TotalMilliseconds);
                        if (process.ExitCode != 0)
                        {
                            _ = SendSlackMessage($"{repo} command {stringCommand} exited with code {process.ExitCode}");
                            _ = SendSlackMessage(errorOutput);
                        }
                        if (!waitResult)
                        {
                            _ = SendSlackMessage($"{repo} command {stringCommand} timed out after {waitTime}");
                            _ = SendSlackMessage(errorOutput);
                        }
                    }
                    _ = SendSlackMessage($"website-ci finished update of {repo}");
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
