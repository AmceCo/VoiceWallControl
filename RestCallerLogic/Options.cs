using CommandLine;
using CommandLine.Text;

namespace RestCallerLogic
{
	public class Options
	{
		[Option('a', "alpha", DefaultValue = "", Required = true, HelpText = "Alpha")]
		public string Alpha { get; set; }

		[Option('p', "password", DefaultValue = "", Required = false, HelpText = "Password for the inbox")]
		public string Password { get; set; }

		[Option('s', "server", DefaultValue = "", Required = false, HelpText = "Inbox Server")]
		public string Server { get; set; }

		[Option('u', "user", DefaultValue = "", Required = false, HelpText = "Username for the inbox")]
		public string Username { get; set; }

		[Option('e', "encrypt", DefaultValue = false, Required = false, HelpText = "Use SSL")]
		public bool UseSSL { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}