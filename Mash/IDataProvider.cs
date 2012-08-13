using System;
using System.Collections.Generic;

namespace Mash
{
	public interface IDataProvider
	{
		//bool ChooseMedia(int mashId, int choiceId);
		bool Mash(MashViewModel mash);
		IList<TagViewModel> GetTags(bool requery = false);
		IList<MediaViewModel> GetMedia(string tag = null, bool requery = false);
		IEnumerable<MashViewModel> GetMashes(string tag, int mashCount);
	}
}