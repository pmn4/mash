using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mash
{
	public class MashViewModel
	{
		public int MashId { get; internal set; }
		public List<MediaViewModel> Media { get; set; }

		public int? ChoiceMediaId { get; set; }

		/// <summary>
		/// Indicates whether the Media in question is of the same subject
		/// </summary>
		public bool Same { get; set; }
	}
}