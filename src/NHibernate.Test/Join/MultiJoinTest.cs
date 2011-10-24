using System.Linq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NHibernate.Test.Join
{
	[TestFixture]
	public class MultipleJoinTest : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get
			{
				return new[]
				       {
				       	"Join.Candidate.hbm.xml",
				       };
			}
		}

		protected override void OnTearDown()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				//deleting in sql because the sql-level interference in one of the tests mucks with s.Delete()
				ExecuteCommand(s, "delete from job_time_range");
				ExecuteCommand(s, "delete from job");
				ExecuteCommand(s, "delete from candidate_email");
				ExecuteCommand(s, "delete from candidate");

				tx.Commit();
			}
		}

		[Test]
		public void JoinedJoins()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				CreateAndSaveCandidate(s);
				s.Clear();

				var candidates = GetCandidatesAndJobs(s);

				Assert.AreEqual(1, candidates.Count);
				var candidate = candidates[0];
				Assert.AreEqual(2, candidate.Jobs.Count);
				Assert.NotNull(candidate.Jobs.Where(j => j.Finish == new DateTime(2006, 10, 14)).FirstOrDefault());

				tx.Commit();
			}
		}

		[Test]
		public void JoinedJoinsUseDerivedTables()
		{

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				CreateAndSaveCandidate(s);
				ExecuteCommand(s, "delete from job_time_range where Start = '2004-5-25'");

				s.Clear();

				var candidates = GetCandidatesAndJobs(s);

				Assert.AreEqual(1, candidates.Count);
				var candidate = candidates[0];
				Assert.AreEqual(1, candidate.Jobs.Count);
				Assert.AreEqual(new DateTime(2006, 10, 20), candidate.Jobs[0].Start);
				tx.Commit();
			}
		}

		private static void CreateAndSaveCandidate(ISession session)
		{
			var firstJob = new Job { Name = "Lasersoft, Inc", Start = new DateTime(2004, 5, 25), Finish = new DateTime(2006, 10, 14) };
			var nextJob = new Job { Name = "Megacorp, LLC", Start = new DateTime(2006, 10, 20), Finish = new DateTime(2011, 5, 25) };

			var candidate = new Candidate { Name = "Ben Bitdiddle", Email = "bbitdiddle@example.com", Jobs = new[] { firstJob, nextJob } };
			firstJob.Candidate = candidate;
			nextJob.Candidate = candidate;
			session.Save(candidate);
		}

		private static void ExecuteCommand(ISession session, string sql)
		{
			using (var command = session.Connection.CreateCommand())
			{
				session.Transaction.Enlist(command);
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}

		private static IList<Candidate> GetCandidatesAndJobs(ISession s)
		{
			return s
				.CreateCriteria<Candidate>()
				.SetFetchMode("Jobs", FetchMode.Join)
				.SetResultTransformer(Transform.Transformers.DistinctRootEntity)
				.List<Candidate>();
		}
	}
}