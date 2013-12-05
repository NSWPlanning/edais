using System;

namespace ETH.Util
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
	}

	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime Now
		{
			get { return DateTime.Now; }
		}
	}
}