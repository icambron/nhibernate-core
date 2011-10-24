﻿using System.Collections.Generic;

namespace NHibernate.Test.Join
{
	public class Candidate
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }
		public virtual string Email { get; set; }

		public virtual IList<Job> Jobs { get; set; }
	}
}