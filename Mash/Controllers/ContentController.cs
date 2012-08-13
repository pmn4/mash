using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mash
{
	public class ContentController : Controller
	{
		//
		// GET: /Content/

		public const string UrlTokenPath = "p";
		public const string TemportaryImagePath = "C:/Users/pnewell/Pictures/camera/1024x683/";

		public ActionResult Fetch(string type = null)
		{
			var path = Request[UrlTokenPath];
			if(string.IsNullOrWhiteSpace(path))
				return new HttpNotFoundResult("Must specify a path");

			switch(type)
			{
				case "image":
					return new FilePathResult(TemportaryImagePath + path, "image/jpeg");
				default:
					return new HttpNotFoundResult("Type is Null");
			}
		}

	}
}
