using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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
		readonly StreamWriter writer;

		readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Converters = new[]
			{
				new StringEnumConverter { CamelCaseText = true }
			}
		};

		public Output(Stream outputStream)
		{
			writer = new StreamWriter(outputStream);
		}

		public void Exception(Exception exception)
		{
			Display(exception);
		}

		public void String(string str)
		{
			writer.WriteLine(str);
			writer.Flush();
		}

		public void Display(object model)
		{
			var json = JsonConvert.SerializeObject(model, jsonSettings);
			String(json);
		}

		public void Dispose()
		{
			writer.Dispose();
		}
	}
}