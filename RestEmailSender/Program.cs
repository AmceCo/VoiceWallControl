using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveUp.Net.Mail;
using CineNet.Common.Extensions;
using RestCallerLogic;
using Parser = CommandLine.Parser;

namespace RestEmailSender
{
    class Program
    {
        private static MailRepository mailRepository;

        private static Options options;
        private static RestApplier applier;

        public static List<string> MessageSubjects { get; set; }

        static void Main(string[] args)
        {
            ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, "Hello World");

            try
            {
                options = new Options();
                

                if (Parser.Default.ParseArguments(args, options))
                {
                    //applier = new RestApplier(options);

                    ConsoleExtensions.WriteLineWithColor(ConsoleColor.Magenta, $"Username: {options.Username} | Password: {options.Password} | Server: {options.Server} | UseSSL: {options.UseSSL}");

                    mailRepository = new MailRepository(options.Server, true, options.Username, options.Password);

                    Task.Run(() => MonitorInbox());

	                Task.Run(() => SendMessage());

                    ConsoleExtensions.WriteLineWithColor(ConsoleColor.Gray, "Press Q to exit");

                    while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
                }
            }
            catch (Exception ex)
            {
                ex.PrintToConsole();
            }
        }

	    private static void SendMessage()
	    {
		    mailRepository.SendMessage("This will be the subject", "I am going to send this to my gmail lets see how it works", "dbasarab617@gmail.com");
	    }

	    private static void MonitorInbox()
        {
            while (true)
                try
                {
                    ProcessInbox();
                }
                catch (Exception ex)
                {
                    ex.PrintToConsole();
                }
        }

        private static void ProcessInbox()
        {
            Task.Delay(TimeSpan.FromSeconds(1));

            var messages = mailRepository.GetAllMails("inbox");

            if (MessageSubjects == null)
            {
                MessageSubjects = new List<string>();

	            foreach (Message message in messages)
	            {
		            MessageSubjects.Add(message.Subject);

					message.PrintMessage();
	            }
            }
            else
                foreach (Message message in messages)
                {
                    if (MessageSubjects.Contains(message.Subject)) continue;

                    MessageSubjects.Add(message.Subject);

                    var delete = applier.ProcessRequest(message.Subject);

                    if (delete) mailRepository.DeleteMessage(message);
                }
        }
    }
}