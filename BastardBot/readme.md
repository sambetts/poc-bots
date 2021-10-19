
BastardBot is a bot that insults people & learns from insults given back, built just for the fun of it. 
Currently hosted @ https://bastard.bot/
# Technology stack
*	Azure Bot Service
    *	Hosts the bot; written in ASP.Net Core 3.1
*	LUIS.
    *	Detects intent (insult, praise, etc)
*	QnA
    *	Database for insults back.
*   Azure functions
    *   AI training routine for new insults, done on a schedule. 
*   React/Node
    *   Public facing website.
Projects are built in Visual Studio 2019.
## Required Configuration/Resources
*	Azure bot service.
*	LUIS application.
*	QnA application.
*	Recommended: SMTP configuration for contact form.

# Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

# License
MIT