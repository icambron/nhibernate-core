using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Test.Join
{
	public class Job
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }
		public virtual DateTime Start { get; set; }
		public virtual DateTime Finish { get; set; }

		public virtual Candidate Candidate { get; set; }
	}
}
