using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mash
{
	public class MashController : Controller
	{
		private readonly IDataProvider _dataProvider = new DataProvider();

		public ActionResult Index(MashDisplayViewModel model)
		{
			model = model ?? new MashDisplayViewModel();
			var tag = model.Tag == null ? DataProvider.DefaultTag : model.Tag.Slug;

			var mashes = _dataProvider.GetMashes(tag, model.Count);
			return View("Mashes", mashes);
		}

		[HttpGet]
		public ActionResult Mashes(MashDisplayViewModel model)
		{
			return Index(model);
		}
		
		[HttpPost]
		public ActionResult Mash(MashViewModel viewModel)
		{
			var choice = viewModel.Media.FirstOrDefault(m => m.MediaId == viewModel.ChoiceMediaId);
			if (choice == null)
			{
				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			var success = _dataProvider.Mash(viewModel);
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
