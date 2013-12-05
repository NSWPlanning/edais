using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCrunch.Framework
{
	public abstract class ResourceUsageAttribute : Attribute
	{
		private readonly string[] _resourceNames;

		public ResourceUsageAttribute(params string[] resourceName)
		{
			_resourceNames = resourceName;
		}

		public string[] ResourceNames
		{
			get { return _resourceNames; }
		}
	}

	public class ExclusivelyUsesAttribute : ResourceUsageAttribute
	{
		public ExclusivelyUsesAttribute(params string[] resourceName)
			: base(resourceName) { }
	}
}