using System;
using System.Collections.Generic;
using System.Linq;
using CineNet.Common.Extensions;
using CineNet.Communication.Web.Client;
using CineNet.Tests.Fakes.CineRest.ServiceModels;
using CineNet.Tests.Fakes.CineRest.ServiceModels.Requests;
using CineNet.Tests.Fakes.RoomServices.Models;
using Newtonsoft.Json;

namespace RestCallerLogic
{
	public class RestApplier
	{
		private readonly Options options;

		public List<DisplayServiceModel> displayList;
		private ExternalWebCaller webCaller;

		public RestApplier(Options options)
		{
			this.options = options;

			try
			{
				CreateWebCaller();

				GetDisplayInfo();
			}
			catch (Exception ex)
			{
				ex.PrintToConsole();
			}
		}

		public List<string> GetLayoutsForDisplay(DisplayServiceModel display)
		{
			var layoutResponse = webCaller.GetFromUrlSync($"Display/{display.DisplayId}/Layout");

			var layouts = JsonConvert.DeserializeObject<List<Layout>>(layoutResponse.Text);

			if (layouts.Count == 0) return new List<string>();

			return layouts.Select(i => i.Name).ToList();
		}

		public bool ProcessRequest(string text)
		{
			text = CleanText(text);

			var displayNames = displayList.Select(i => i.Name).ToList();

			var hasDisplayName = displayNames.Any(i => text.ToLower().Contains(i.ToLower()));

			if (hasDisplayName) ConsoleExtensions.WriteLineWithColor(ConsoleColor.Green, $"({text}) |{string.Join(",", displayNames)}");
			else ConsoleExtensions.WriteLineWithColor(ConsoleColor.Magenta, $"({text}) |{string.Join(",", displayNames)}");

			if (!hasDisplayName) return false;

			var display = displayList.FirstOrDefault(i => text.ToLower().Contains(i.Name.ToLower()));

			if (text.ToLower().Contains("clear"))
			{
				ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Going to clear the display ({display.Name} | {display.DisplayId})");

				var request = new ClearCanvasRequest
				              {
					              CanvasId = display.CanvasId.ToString(),
					              WorkspaceId = display.WorkspaceId.ToString()
				              };

				webCaller.PostToUrlSync($"Display/{display.DisplayId}/Window/Clear", request.ToJson());

				return true;
			}

			//			if (text.ToLower().Contains("play"))
			//			{
			//				webCaller.GetFromUrlSync($"Display/{display.DisplayId}/PlayTimeline");
			//
			//				return true;
			//			}
			//
			//			if (text.ToLower().Contains("stop"))
			//			{
			//				webCaller.GetFromUrlSync($"Display/{display.DisplayId}/StopTimeline");
			//
			//				return true;
			//			}

			var layoutResponse = webCaller.GetFromUrlSync($"Display/{display.DisplayId}/Layout");

			var layouts = JsonConvert.DeserializeObject<List<Layout>>(layoutResponse.Text);

			if (layouts.Count == 0) return false;

			var foundLayout = layouts.FirstOrDefault(i => text.ToLower().Contains(i.Name.ToLower()));

			if (foundLayout != null)
			{
				webCaller.PostToUrlSync($"Display/{display.DisplayId}/Layout/{foundLayout.LayoutId}/Apply", string.Empty);

				ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Going to apply layout {foundLayout.Name} | {foundLayout.LayoutId} to display ({display.Name} | {display.DisplayId})");

				return true;
			}

			return false;
		}

		private string CleanText(string text)
		{
			var cleanText = text;

			if (cleanText.ToLower().Contains("one")) cleanText = cleanText.Replace("one", "1");

			return cleanText;
		}

		private void CreateWebCaller()
		{
			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Green, $"Using Alpha {options.Alpha}");

			var uri = new Uri($"http://{options.Alpha}:25002/CineNet/NetworkManager");

			var args = new ExternalWebCallerArguments
			           {
				           BaseUrl = uri.LocalPath,
				           IpAddress = uri.Host,
				           SendWithSSL = false,
				           Port = (ushort)uri.Port,
				           Version = "1.0",
				           AuthorizationToken = "hOKjrxOmPxysgSPmvvCJSy9d1SsdLfp3DMAZqDtZ"
			           };

			webCaller = new ExternalWebCaller(args);
		}

		private void GetDisplayInfo()
		{
			var response = webCaller.GetFromUrlSync("Display");

			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, $"Network Manager response: {response.Text}");

			displayList = JsonConvert.DeserializeObject<List<DisplayServiceModel>>(response.Text);

			foreach (var display in displayList) ConsoleExtensions.WriteLineWithColor(ConsoleColor.Green, $"Display: {display.Name} | Id: {display.DisplayId}");
		}
	}
}