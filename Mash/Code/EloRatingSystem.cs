using System;
using System.Linq;

namespace Mash.Code
{
	public class EloRatingSystem
	{
		public const decimal EloConstant = 1200;
		public const string RatingType = "elo";

		public static bool UpdateRatings(MashViewModel model)
		{
			if (model == null || model.Media == null || model.Media.Count != 2)
				return false;

			var mediaA = model.Media.ElementAt(0);
			var mediaB = model.Media.ElementAt(1);

			if (mediaA == null || mediaB == null)
				return false;

			mediaA.RatingType = RatingType;
			mediaB.RatingType = RatingType;
			if(!mediaA.CurrentRating.HasValue)
				mediaA.CurrentRating = EloConstant;
			if(!mediaB.CurrentRating.HasValue)
				mediaB.CurrentRating = EloConstant;

			double e;

			if (mediaA.MediaId == model.ChoiceMediaId)
			{
				e = 120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaB.CurrentRating - mediaA.CurrentRating) / EloConstant))) * 120);
				mediaA.Rating = mediaA.CurrentRating + (decimal) e;
				mediaB.Rating = mediaB.CurrentRating - (decimal) e;
			}
			else if (mediaB.MediaId == model.ChoiceMediaId)
			{
				e = 120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaA.CurrentRating - mediaB.CurrentRating) / EloConstant))) * 120);
				mediaB.Rating = mediaB.CurrentRating + (decimal) e;
				mediaA.Rating = mediaA.CurrentRating - (decimal) e;
			}
			else
			{
				if (mediaA.CurrentRating == mediaB.CurrentRating)
				{
					mediaA.Rating = mediaA.CurrentRating;
					mediaB.Rating = mediaB.CurrentRating;
				}
				else
				{
					if (mediaA.CurrentRating > mediaB.CurrentRating)
					{
						e = (120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaA.CurrentRating - mediaB.CurrentRating) / EloConstant))) * 120)) -
							(120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaB.CurrentRating - mediaA.CurrentRating) / EloConstant))) * 120));
						mediaA.Rating = mediaA.CurrentRating - (decimal) e;
						mediaB.Rating = mediaB.CurrentRating + (decimal) e;
					}
					else
					{
						e = (120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaB.CurrentRating - mediaA.CurrentRating) / EloConstant))) * 120)) -
							(120 - Math.Round(1 / (1 + Math.Pow(10, (double)((mediaA.CurrentRating - mediaB.CurrentRating) / EloConstant))) * 120));
						mediaA.Rating = mediaA.CurrentRating + (decimal) e;
						mediaB.Rating = mediaB.CurrentRating - (decimal) e;
					}
				}
			}

			return true;
		}
	}
}
