using System;
using System.Collections.Generic;

namespace Mash
{
	public interface IDataProvider
	{
		//bool ChooseMedia(int mashId, int choiceId);
		bool Mash(MashViewModel mash);
		IList<TagViewModel> GetTags(bool requery = false);
		IList<MediaViewModel> GetMedia(IEnumerable<string> tags = null, int? mediaCount = null, bool ranked = false, bool requery = false);
		IEnumerable<MashViewModel> GetMashes(IEnumerable<string> tags = null, int? mashCount = null);

		IEnumerable<MashViewModel> GetResults(int mashCount);
		bool UpdateMashAddRating(MashViewModel mash);
	}
}