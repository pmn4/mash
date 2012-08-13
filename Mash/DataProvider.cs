using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Mash
{
	public class DataProvider : IDataProvider
	{
		public const int GetMashesMax = 10;
		public const string DefaultTag = "all";
		private readonly MySqlConnection _conn;
		private IList<TagViewModel> _tags;
		private IList<MediaViewModel> _media;

		public DataProvider()
		{
			_conn = new MySqlConnection(ConfigurationManager.AppSettings["ConnectionString"]);
			_conn.Open();
		}

		~DataProvider()
		{
			_conn.Close();
		}

		public IList<TagViewModel> GetTags(bool requery = false)
		{
			if (requery || _tags == null)
				_tags = FetchTags();

			return _tags;
		}
		private IList<TagViewModel> FetchTags()
		{
			var models = new List<TagViewModel>();
			var sql = "SELECT inx, slug, name FROM mashegory;";
			var command = new MySqlCommand(sql, _conn);

#warning use prepared statements or other
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					models.Add(new TagViewModel
					{
						TagId = (int)((uint)reader.GetValue(0)),
						Slug = (string)reader.GetValue(1),
						Name = (string)reader.GetValue(2),
					});
				}
			}

			return models;
		}

		private IList<MediaViewModel> GetRandomMedia(int mashCount)
		{
			var models = new List<MediaViewModel>();
			var sql = string.Format(
				@"SELECT m.inx, m.name, m.url, m.type,
				         RAND() * x.randomizer 'rand_ind'
				  FROM   media m, (SELECT MAX(t.inx) - 1 'randomizer' FROM media t) x
				  ORDER BY rand_ind
				  LIMIT  {0};",
				mashCount);

#warning use prepared statements or other
			var command = new MySqlCommand(sql, _conn);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					models.Add(new MediaViewModel
						        {
									MediaId = (int)((uint)reader.GetValue(0)),
									Name = reader.IsDBNull(1) ? null : (string)reader.GetValue(1),
									Url = (string)reader.GetValue(2),
									Type = (string)reader.GetValue(3),
						        });
				}
			}

			return models;
		}

		private bool AddMashes(string tag, int mashCount, string guid)
		{
			var mediaa = GetRandomMedia(mashCount);
			var mediab = GetRandomMedia(mashCount);
			var sql = "INSERT INTO mashup(mediaa, mediab, grouping) VALUES";
			for (var i = 0; i < mashCount; i++)
			{
				if (i > 0) sql += ',';
				sql += String.Format("({0},{1},'{2}')",
				                     mediaa.ElementAt(i).MediaId,
				                     mediab.ElementAt(i).MediaId,
				                     guid);
			}

			sql += ';';
			var command = new MySqlCommand(sql, _conn);
			return command.ExecuteNonQuery() > 0;
		}

		public bool Mash(MashViewModel mash)
		{
			var sql = String.Format(
				"INSERT INTO mashup(mediaa, mediab, choice, timestamp) VALUES({0}, {1}, {2}, CURRENT_TIMESTAMP);",
				mash.Media.ElementAt(0).MediaId,
				mash.Media.ElementAt(1).MediaId,
				mash.ChoiceMediaId);

			var command = new MySqlCommand(sql, _conn);
			return command.ExecuteNonQuery() > 0;
		}
		public IEnumerable<MashViewModel> GetMashes(string tag = DefaultTag, int mashCount = GetMashesMax)
		{
			mashCount = Math.Min(GetMashesMax, mashCount);
			var models = new List<MashViewModel>();

			var media = GetMedia(tag);
			var rand = new Random();

			for (var i = 0; i < mashCount; i++)
			{
				models.Add(new MashViewModel
							{
								Media = new List<MediaViewModel>
								        	{
								        		media.ElementAt(rand.Next(media.Count)),
												media.ElementAt(rand.Next(media.Count)),
								        	}
							});
			}

			return models;
		}

//        public IEnumerable<MashViewModel> GetMashes(string tag = DefaultTag, int mashCount = GetMashesMax)
//        {
//            mashCount = Math.Min(GetMashesMax, mashCount);
//            var models = new List<MashViewModel>();

//            var guid = Guid.NewGuid().ToString("n");
//            if (!AddMashes(tag, mashCount, guid))
//            {
//                //print some error
//                return null;
//            }


//#warning use prepared statements or other
//            var sql = string.Format(
//                @"SELECT m.inx,
//				         a.inx,
//				         a.name,
//				         a.url,
//				         a.type,
//				         b.inx,
//				         b.name,
//				         b.url,
//				         b.type
//				  FROM mashup m, media a, media b
//				  WHERE  a.inx = m.mediaa
//				  AND    b.inx = m.mediab
//				  AND    m.choice IS NULL
//				  AND    m.grouping = '{1}'
//				  LIMIT {0};",
//                mashCount, guid);
//            var command = new MySqlCommand(sql, _conn);
//            using (var reader = command.ExecuteReader())
//            {
//                while (reader.Read())
//                {
//                    models.Add(new MashViewModel
//                                {
//                                    MashId = (int)((uint)reader.GetValue(0)),
//                                    Media = new List<MediaViewModel>
//                                                {
//                                                    new MediaViewModel
//                                                        {
//                                                            MediaId = (int)((uint)reader.GetValue(1)),
//                                                            Name = reader.IsDBNull(2) ? null : (string)reader.GetValue(2),
//                                                            Url = reader.GetString(3),
//                                                            Type = reader.GetString(4),
//                                                        },
//                                                    new MediaViewModel
//                                                        {
//                                                            MediaId = (int)((uint)reader.GetValue(5)),
//                                                            Name = reader.IsDBNull(6) ? null : (string)reader.GetValue(6),
//                                                            Url = reader.GetString(7),
//                                                            Type = reader.GetString(8),
//                                                        },
//                                                },
//                                }
//                        );
//                }
//            }

//            return models;
//        }

		public IList<MediaViewModel> GetMedia(string tag = null, bool requery = false)
		{
			if (requery || _media == null)
				_media = FetchMedia();

			return _media;
			return tag == null ? _media : _media.Where(m => m.Tags.Any(c => c.Slug == tag)).ToList();
		}
		private IList<MediaViewModel> FetchMedia()
		{ 

			var models = new List<MediaViewModel>();
			var sql = "SELECT inx, name, type, url FROM media;";
			var command = new MySqlCommand(sql, _conn);

#warning use prepared statements or other
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					models.Add(new MediaViewModel
					{
						MediaId = (int)((uint)reader.GetValue(0)),
						Name = reader.IsDBNull(1) ? null : (string)reader.GetValue(1),
						Type = (string)reader.GetValue(2),
						Url = (string)reader.GetValue(3),
					});
				}
			}

			return (_media = models);
		}

		public bool ChooseMedia(int mashId, int choiceId)
		{
#warning use prepared statements or other
			var sql = string.Format(
				@"UPDATE mashup
				  SET    choice = {1}
				  WHERE  inx = {0};",
				mashId, choiceId);

			var command = new MySqlCommand(sql, _conn);
			return command.ExecuteNonQuery() == 1;
		}
	}
}