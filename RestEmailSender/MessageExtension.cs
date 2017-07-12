using System;
using ActiveUp.Net.Mail;
using CineNet.Common.Extensions;

namespace RestEmailSender
{
	public static class MessageExtension
	{
		public static void PrintMessage(this Message message)
		{
			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Cyan, $"Message | Subject: {message.Subject}");
		}
	}
}