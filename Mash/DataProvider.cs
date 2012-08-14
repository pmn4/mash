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

		private bool AddMashes(IEnumerable<string> tags, int mashCount, string guid)
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
				"INSERT INTO mashup(mediaa, mediaarating, mediab, mediabrating, choice, timestamp) VALUES({0}, {1}, {2}, {3}, {4}, CURRENT_TIMESTAMP);",
				mash.Media.ElementAt(0).MediaId,
				mash.Media.ElementAt(0).CurrentRating,
				mash.Media.ElementAt(1).MediaId,
				mash.Media.ElementAt(1).CurrentRating,
				mash.ChoiceMediaId);

			var command = new MySqlCommand(sql, _conn);
			var mashResult = command.ExecuteNonQuery() > 0;
			var mashId = command.LastInsertedId;
			var mediaA = mash.Media.ElementAt(0);
			var mediaB = mash.Media.ElementAt(1);

			sql =
				String.Format(
					"INSERT INTO rating (type, media, rating, increase, mashup) VALUES ('{0}', {1}, {2}, {3}, {4}), ('{5}', {6}, {7}, {8}, {9});",
					mediaA.RatingType,
					mediaA.MediaId,
					mediaA.Rating,
					mediaA.CurrentRating - mediaA.Rating,
					mashId,
					mediaB.RatingType,
					mediaB.MediaId,
					mediaB.Rating,
					mediaB.CurrentRating - mediaB.Rating,
					mashId);
			command.Dispose();
			command = new MySqlCommand(sql, _conn);

			return mashResult && command.ExecuteNonQuery() > 0;
		}
		public bool UpdateMashAddRating(MashViewModel mash)
		{
			var sql = String.Format(
				"UPDATE mashup SET mediaa = {0}, mediaarating = {1}, mediab = {2}, mediabrating = {3}, choice = {4} WHERE inx = {5};",
				mash.Media.ElementAt(0).MediaId,
				mash.Media.ElementAt(0).CurrentRating,
				mash.Media.ElementAt(1).MediaId,
				mash.Media.ElementAt(1).CurrentRating,
				mash.ChoiceMediaId,
				mash.MashId);

			var command = new MySqlCommand(sql, _conn);
			var mashResult = command.ExecuteNonQuery() > 0;
			var mediaA = mash.Media.ElementAt(0);
			var mediaB = mash.Media.ElementAt(1);

			sql =
				String.Format(
					"INSERT INTO rating (type, media, rating, increase, mashup) VALUES ('{0}', {1}, {2}, {3}, {4}), ('{5}', {6}, {7}, {8}, {9});",
					mediaA.RatingType,
					mediaA.MediaId,
					mediaA.Rating,
					mediaA.CurrentRating - mediaA.Rating,
					mash.MashId,
					mediaB.RatingType,
					mediaB.MediaId,
					mediaB.Rating,
					mediaB.CurrentRating - mediaB.Rating,
					mash.MashId);
			command.Dispose();
			command = new MySqlCommand(sql, _conn);

			return mashResult && command.ExecuteNonQuery() > 0;
		}
		public IEnumerable<MashViewModel> GetMashes(IEnumerable<string> tags = null, int? mashCount = null)
		{
			mashCount = Math.Min(GetMashesMax, mashCount ?? GetMashesMax);
			var models = new List<MashViewModel>();

			var media = GetMedia(tags ?? new List<string> { DefaultTag });
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

		public IEnumerable<MashViewModel> GetResults(int mashCount = GetMashesMax)
		{
			var models = new List<MashViewModel>();
			var sql = String.Format(
				@"SELECT m.inx, m.choice, m.same, a.inx, a.name, a.url, a.type, ra.rating, b.inx, b.name, b.url, b.type, rb.rating
FROM   mashup m
INNER JOIN media a
ON     a.inx = m.mediaa
INNER JOIN media b
ON     b.inx = m.mediab
LEFT OUTER JOIN rating ra
ON     ra.media = a.inx
AND    ra.ts = (SELECT MAX(ra2.ts) FROM rating ra2 WHERE ra2.media = a.inx)
LEFT OUTER JOIN rating rb
ON     rb.media = b.inx
AND    rb.ts = (SELECT MAX(rb2.ts) FROM rating rb2 WHERE rb2.media = b.inx)
WHERE NOT EXISTS(
  SELECT 1
  FROM rating r
  WHERE r.mashup = m.inx
)
LIMIT {0};", mashCount);

#warning use prepared statements or other
			var command = new MySqlCommand(sql, _conn);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var i = -1;
					models.Add(new MashViewModel
					           	{
					           		MashId = (int) ((uint) reader.GetValue(++i)),
					           		ChoiceMediaId = (int) ((uint) reader.GetValue(++i)),
					           		Same = (bool) reader.GetValue(++i),
					           		Media = new List<MediaViewModel>
					           		        	{
					           		        		new MediaViewModel
					           		        			{
					           		        				MediaId = (int) ((uint) reader.GetValue(++i)),
					           		        				Name = reader.IsDBNull(++i) ? null : (string) reader.GetValue(i),
					           		        				Url = (string) reader.GetValue(++i),
					           		        				Type = (string) reader.GetValue(++i),
															CurrentRating = reader.IsDBNull(++i) ? (decimal?) null : (decimal) reader.GetValue(i)
					           		        			},
					           		        		new MediaViewModel
					           		        			{
					           		        				MediaId = (int) ((uint) reader.GetValue(++i)),
					           		        				Name = reader.IsDBNull(++i) ? null : (string) reader.GetValue(i),
					           		        				Url = (string) reader.GetValue(++i),
					           		        				Type = (string) reader.GetValue(++i),
															CurrentRating = reader.IsDBNull(++i) ? (decimal?) null : (decimal) reader.GetValue(i)
					           		        			}
					           		        	}
					           	});
				}
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

		public IList<MediaViewModel> GetMedia(IEnumerable<string> tags = null, int? mediaCount = null, bool ranked = false, bool requery = false)
		{
#warning cache?
			return FetchMedia(tags, mediaCount, ranked);
		}

#warning add tags support
		private IList<MediaViewModel> FetchMedia(IEnumerable<string> tags = null, int? mediaCount = null, bool ranked = false)
		{ 

			var models = new List<MediaViewModel>();
			var sql = String.Format(
				@"SELECT m.inx, m.name, m.type, m.url, r.rating
FROM media m
LEFT OUTER JOIN rating r
ON     r.media = m.inx
AND    r.ts = (SELECT MAX(r2.ts) FROM rating r2 WHERE r2.media = m.inx)");
			if (ranked)
				sql += " ORDER BY r.rating DESC";
			if (mediaCount.HasValue)
				sql += String.Format(" LIMIT {0}", mediaCount);

			var command = new MySqlCommand(sql, _conn);

#warning use prepared statements or other
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var i = -1;
					models.Add(new MediaViewModel
					{
						MediaId = (int)((uint)reader.GetValue(++i)),
						Name = reader.IsDBNull(++i) ? null : (string)reader.GetValue(i),
						Type = (string)reader.GetValue(++i),
						Url = (string)reader.GetValue(++i),
						CurrentRating = reader.IsDBNull(++i) ? (decimal?) null : (decimal) reader.GetValue(i),
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