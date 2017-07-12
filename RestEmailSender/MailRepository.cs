using System;
using System.Net;
using System.Net.Mail;
using ActiveUp.Net.Mail;
using CineNet.Common.Extensions;
using SmtpClient = ActiveUp.Net.Mail.SmtpClient;

namespace RestEmailSender
{
	public class MailRepository : IDisposable
	{
		private readonly string login;
		private readonly string mailServer;
		private readonly string password;
		private readonly bool ssl;
		private Imap4Client client;

		protected Imap4Client Client => client ?? (client = new Imap4Client());

		public MailRepository(string mailServer, bool ssl, string login, string password)
		{
			this.mailServer = mailServer;
			this.ssl = ssl;
			this.login = login;
			this.password = password;

			if (ssl) Client.ConnectSsl(mailServer);
			else Client.Connect(mailServer);

			Client.Login(login, password);

			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Green, "After Login");

			RegisterForEvents();
		}

		public void DeleteMessage(Message message)
		{
			var mailBox = Client.SelectMailbox("Inbox");

			var messages = mailBox.SearchParse("ALL");

			for (var index = 0; index < messages.Count; index++)
			{
				var currentMessage = messages[index];

				if (message.Subject == currentMessage.Subject)
				{
					ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Index on Server = {message.IndexOnServer} | Id = {message.Id} | Deleteing Index = {index}");

					mailBox.DeleteMessage(index + 1, true);
				}
			}
		}

		public void Dispose()
		{
			Client?.Close();
		}

		public MessageCollection GetAllMails(string mailBox)
		{
			return GetMails(mailBox, "ALL");
		}

		public MessageCollection GetUnreadMails(string mailBox)
		{
			return GetMails(mailBox, "UNSEEN");
		}

		public void PrintInbox()
		{
			var emailList = GetAllMails("inbox");

			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Found {emailList.Count} emails");

			foreach (Message message in emailList) ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Message: Subject: {message.Subject}");
		}

		public void SendMessage(string subject, string message, string emailAddress)
		{
			try
			{
				ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Going to Send Message | Subject = {subject} | Message = {message} | Address = {emailAddress} | ADDING FROM this time");

				//var temp = new SmtpMessage();

				//temp.BodyText.Text = message;

				//temp.Subject = subject;

				//temp.To.Add(emailAddress);
				//temp.From = new Address("david.basarab@cinemassive.com");

				//temp.SendSsl(mailServer, this.login, this.password, SaslMechanism.Login);



				MailMessage mail = new MailMessage();
				mail.From = new System.Net.Mail.MailAddress("david.basarab@cinemassive.com");

				// The important part -- configuring the SMTP client
				var smtp = new System.Net.Mail.SmtpClient();
				smtp.Port = 587;   // [1] You can try with 465 also, I always used 587 and got success
				smtp.EnableSsl = true;
				smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // [2] Added this
				smtp.UseDefaultCredentials = false; // [3] Changed this
				smtp.Credentials = new NetworkCredential(login, password);  // [4] Added this. Note, first parameter is NOT string.
				smtp.Host = this.mailServer;

				//recipient address
				mail.To.Add(new MailAddress(emailAddress));

				//Formatted mail body
				mail.IsBodyHtml = true;
				mail.Body = message;

				smtp.Send(mail);

				ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Done Sending Message");
			}
			catch (Exception ex) 
			{
				ex.PrintToConsole();
			}
		}

		private MessageCollection GetMails(string mailBox, string searchPhrase)
		{
			var mails = Client.SelectMailbox(mailBox);
			var messages = mails.SearchParse(searchPhrase);

			return messages;
		}

		private void OnNewMessageReceived(object sender, NewMessageReceivedEventArgs e)
		{
			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Green, $"New Message {e.MessageCount}");

			PrintInbox();
		}

		private void RegisterForEvents()
		{
			Client.NewMessageReceived += OnNewMessageReceived;
		}
	}
}