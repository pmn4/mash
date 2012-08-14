using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mash.Code;

namespace Mash
{
	public class MashController : Controller
	{
		private readonly IDataProvider _dataProvider = new DataProvider();

		public ActionResult Index(MashDisplayViewModel model)
		{
			model = model ?? new MashDisplayViewModel();

			var mashes = _dataProvider.GetMashes(model.Tags ==  null ? null : model.Tags.Select(t => t.Slug), model.Count);
			return View("Mashes", mashes);
		}

		[HttpGet]
		public ActionResult Mashes(MashDisplayViewModel model)
		{
			return Index(model);
		}

		[HttpGet]
		public ActionResult Debug()
		{
			const int iterations = 20;
			var model = new List<MashViewModel>();
			for (var i = 0; i < iterations; i++)
			{
				var mashups = _dataProvider.GetResults(1).ToList();
				model.AddRange(mashups);
				foreach (var m in mashups)
				{
					EloRatingSystem.UpdateRatings(m);
					_dataProvider.UpdateMashAddRating(m);
				}
			}

			return View("Debug", model);
		}

		[HttpGet]
		public ActionResult Leaderboard(int count) // accept a list of tags
		{
			var model = _dataProvider.GetMedia(null, count, true, true).OrderByDescending(m => m.CurrentRating ?? EloRatingSystem.EloConstant);

			// test for ajax
			return View("Leaderboard", model.ToList());
		}
		
		[HttpPost]
		public ActionResult Mash(MashViewModel viewModel)
		{
			var choice = viewModel.Media.FirstOrDefault(m => m.MediaId == viewModel.ChoiceMediaId);
			if (choice == null)
			{
				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}

			var success = EloRatingSystem.UpdateRatings(viewModel);

			if(success)
				success = _dataProvider.Mash(viewModel);

			foreach(var m in viewModel.Media.Where(m => m.Rating.HasValue))
			{
				m.CurrentRating = m.Rating.Value; // nice try resharper
				m.Rating = null;
			}

			if (ControllerContext.RequestContext.HttpContext.Request.IsAjaxRequest())
			{
				return Json(new
				{
					success,
				}, JsonRequestBehavior.AllowGet);
			}

			return new RedirectResult(Url.RouteUrl("Default"));
		}
	}
}
