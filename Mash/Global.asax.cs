using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Mash
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default",
				"",
				new { controller = "Mash", action = "Index" }
			);

			routes.MapRoute(
				"Mash",
				"mash",
				new { controller = "Mash", action = "Mash" }
			);

			routes.MapRoute(
				"Leaderboard",
				"leaderboard/{count}",
				new { controller = "Mash", action = "Leaderboard" }
			);

			routes.MapRoute(
				"Fetch",
				"fetch/{type}",
				new { controller = "Content", action = "Fetch" }
			);

			routes.MapRoute(
				"Debug",
				"debug",
				new { controller = "Mash", action = "Debug" }
			);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}