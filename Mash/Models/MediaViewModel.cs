using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mash
{
	public class MediaViewModel
	{
		public int MediaId { get; set; }

		public string Name { get; set; }
		public string Url { get; set; }
#warning use Enum
		public string Type { get; set; }

		public List<TagViewModel> Tags { get; set; }

		public decimal? CurrentRating { get; set; }
		public decimal? Rating { get; set; }
		public string RatingType { get; set; }

		public object Content { get; internal set; }
		public object DataJson { get; internal set; }
	}
}