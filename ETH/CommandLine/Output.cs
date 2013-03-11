using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ServiceStack.Text;
using Utility.Logging;

namespace ETH.CommandLine
{
	public interface IOutput
	{
		void Exception(Exception exception);
		void Display(object model);
		void String(string str);
	}

	class Output : IOutput, IDisposable
	{
		readonly ILogger logger;
		readonly StreamWriter writer;

		readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Converters = new[]
			{
				new StringEnumConverter { CamelCaseText = true }
			}
		};

		public Output(Stream outputStream, ILogger logger)
		{
			this.logger = logger;
			writer = new StreamWriter(outputStream);
		}

		public void Exception(Exception exception)
		{
			Display(exception);
		}

		public void String(string str)
		{
			String(str, true);
		}

		void String(string str, bool log)
		{
			if (log)
			{
				logger.Info(str);
			}
			writer.WriteLine(str);
			writer.Flush();
		}

		public void Display(object model)
		{
			logger.Info(model.Dump());
			var json = JsonConvert.SerializeObject(model, jsonSettings);
			String(json, false);
		}

		public void Dispose()
		{
			writer.Dispose();
		}
	}
}