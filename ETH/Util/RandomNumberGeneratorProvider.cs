using System.Security.Cryptography;

namespace ETH.Util
{
	public interface IRandomNumberGeneratorProvider
	{
		void GetBytes(byte[] data);
	}

	public class RandomNumberGeneratorProvider : IRandomNumberGeneratorProvider
	{
		 public void GetBytes(byte[] data)
		 {
			 RandomNumberGenerator.Create().GetBytes(data);
		 }
	}
}