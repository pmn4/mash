using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mash
{
	public class MashDisplayViewModel
	{
		public const int DefaultCount = 10;

		public int Count { get; set; }
		public List<TagViewModel> Tags { get; set; }

		public MashDisplayViewModel()
		{
			Count = DefaultCount;
		}
	}
}