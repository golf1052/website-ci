using System;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;

namespace website_ci.Controllers
{
    [Route("[controller]")]
    public class WebHookController : Controller
    {
        [HttpPost]
        public string HandleHook([FromBody]JObject o)
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
                    ProcessStartInfo info = new ProcessStartInfo("git", "pull");
                    info.UseShellExecute = false;
                    info.WorkingDirectory = (string)settings[repo]["workingDir"];
                    Process.Start(info);
                }
                return "push";
            }
            else
            {
                return "not push";    
            }
        }
    }
}