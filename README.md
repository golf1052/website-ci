# website-ci

CI for my website

## Setup

1. Install [ASP.NET Core](https://get.asp.net/)
2. Clone
3. Update settings.json with repository name and the root directory of the website
4. Setup a GitHub webhook.
    - Server URL should point to the server (http://example.com/webhook)
    - Content type should be JSON
    - The secret does not need to be set
    - The webhook should be for the push event
