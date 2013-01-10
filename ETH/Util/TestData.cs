using System.Reflection;
using System.Xml.Serialization;
using eDAIS;

namespace ETH.Util
{
	public interface ITestDataLoader
	{
		T FromXml<T>(string resourceName);
	}

	class TestDataLoader : ITestDataLoader
	{
		public T FromXml<T>(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("ETH.Scenarios.Messages." + resourceName))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}
	}
}