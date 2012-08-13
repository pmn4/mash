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
		public TagViewModel Tag { get; set; }

		public MashDisplayViewModel()
		{
			Count = DefaultCount;
		}
	}
}