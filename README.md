# website-ci

CI for my website

## Setup

1. Install [asp.net 5](https://get.asp.net/)
2. Clone
3. Update settings.json with repository name and the root directory of the website
4. Setup Kestrel to run with a script (for Linux). [Info](http://druss.co/2015/06/run-kestrel-in-the-background/) and [script](https://gist.github.com/drussilla/3182463b9fa1ec94b9db)
5. Setup a GitHub webhook.
    - Server URL should point to the server (http://example.com/webhook)
    - Content type should be JSON
    - The secret does not need to be set
    - The webhook should be for the push event
