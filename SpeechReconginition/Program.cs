using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading.Tasks;
using CineNet.Common.Extensions;
using CommandLine;
using RestCallerLogic;

namespace SpeechReconginition
{
	class Program
	{
		private static RestApplier applier;
		private static Options options;
		private static SpeechRecognitionEngine speechRecognitionEngine;

		private static void BuildGrammar()
		{
			var choices = new Choices();

			foreach (var display in applier.displayList)
			{
				choices.Add($"{display.Name} clear");
				choices.Add($"clear {display.Name}");

				//				choices.Add($"{display.Name} play");
				//				choices.Add($"{display.Name} play timeline");
				//				choices.Add($"{display.Name} stop timeline");
				//				choices.Add($"{display.Name} stop");

				var layouts = applier.GetLayoutsForDisplay(display);

				foreach (var layout in layouts)
				{
					choices.Add($"{display.Name} {layout}");
					choices.Add($"{display.Name} apply {layout}");
					choices.Add($"{display.Name} go go {layout}");
				}
			}

			// Create and load a dictation grammar.
			speechRecognitionEngine.LoadGrammar(new Grammar(new GrammarBuilder(choices)));
		}

		static void Main(string[] args)
		{
			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Yellow, "Starting Speech Engine.");

			options = new Options();

			if (Parser.Default.ParseArguments(args, options))
			{
				applier = new RestApplier(options);

				Task.Run(StartSpeechEngine);
			}

			// Keep the console window open.
			Console.ReadKey(true);

			speechRecognitionEngine?.Dispose();
		}

		// Handle the SpeechRecognized event.
		static void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			ConsoleExtensions.WriteLineWithColor(ConsoleColor.Cyan, $"{e.Result.Text} | Confidence = {e.Result.Confidence}");

			if (e.Result.Confidence < .91) return;

			applier.ProcessRequest(e.Result.Text);
		}

		private static async Task StartSpeechEngine()
		{
			// Create an in-process speech recognizer for the en-US locale.
			try
			{
				speechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));

				BuildGrammar();

				// Add a handler for the speech recognized event.
				speechRecognitionEngine.SpeechRecognized += OnSpeechRecognized;

				//recognizer.AudioLevelUpdated += OnAudioLevelUpdated;

				// Configure input to the speech recognizer.
				speechRecognitionEngine.SetInputToDefaultAudioDevice();

				// Start asynchronous, continuous speech recognition.
				speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
			}
			catch (Exception ex)
			{
				ex.PrintToConsole();
			}
		}
	}
}