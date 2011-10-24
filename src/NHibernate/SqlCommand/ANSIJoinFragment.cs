using System;
using NHibernate.Util;

namespace NHibernate.SqlCommand
{
	/// <summary>
	/// An ANSI-style Join.
	/// </summary>
	public class ANSIJoinFragment : JoinFragment
	{
		private SqlStringBuilder buffer = new SqlStringBuilder();
		private readonly SqlStringBuilder conditions = new SqlStringBuilder();

		public override void AddJoin(string tableName, string alias, string[] fkColumns, string[] pkColumns, JoinType joinType)
		{
			AddJoin(tableName, alias, fkColumns, pkColumns, joinType, null);
		}

		public override void AddJoin(string tableName, string alias, string[] fkColumns, string[] pkColumns, JoinType joinType,
									 SqlString on)
		{
			string joinString = GetJoinTypeString(joinType);

			buffer.Add(joinString + tableName + ' ' + alias + " on ");

			AddOnClause(alias, fkColumns, pkColumns);

			AddCondition(buffer, on);
		}

		public override void AddMultiLevelJoin(string tableName, string alias, string[] fkColumns, string[] pkColumns, JoinType joinType,
									 SqlString on, Func<bool, JoinFragment> innerFragment)
		{
			string joinString = GetJoinTypeString(joinType);

			SqlString innerPart = innerFragment(true).ToFromFragmentString;

			buffer.Add(string.Format("{0} ({1} {2} {3}) on ", joinString, tableName, alias, innerPart));
			AddOnClause(alias, fkColumns, pkColumns);

			AddCondition(buffer, on);
		}

		public override SqlString ToFromFragmentString
		{
			get { return buffer.ToSqlString(); }
		}

		public override SqlString ToWhereFragmentString
		{
			get { return conditions.ToSqlString(); }
		}

		public override void AddJoins(SqlString fromFragment, SqlString whereFragment)
		{
			buffer.Add(fromFragment);
			//where fragment must be empty!
		}

		public JoinFragment Copy()
		{
			ANSIJoinFragment copy = new ANSIJoinFragment();
			copy.buffer = new SqlStringBuilder(buffer.ToSqlString());
			return copy;
		}

		public override void AddCrossJoin(string tableName, string alias)
		{
			buffer.Add(StringHelper.CommaSpace + tableName + " " + alias);
		}

		public override bool AddCondition(string condition)
		{
			return AddCondition(conditions, condition);
		}

		public override bool AddCondition(SqlString condition)
		{
			return AddCondition(conditions, condition);
		}

		public override void AddFromFragmentString(SqlString fromFragmentString)
		{
			buffer.Add(fromFragmentString);
		}

		private string GetJoinTypeString(JoinType joinType)
		{
			switch (joinType)
			{
				case JoinType.InnerJoin:
					return " inner join ";
				case JoinType.LeftOuterJoin:
					return " left outer join ";
				case JoinType.RightOuterJoin:
					return " right outer join ";
				case JoinType.FullJoin:
					return " full outer join ";
				default:
					throw new AssertionFailure("undefined join type");
			}
		}

		private void AddOnClause(string alias, string[] fkColumns, string[] pkColumns)
		{
			for (int j = 0; j < fkColumns.Length; j++)
			{
				buffer.Add(fkColumns[j] + "=" + alias + StringHelper.Dot + pkColumns[j]);
				if (j < fkColumns.Length - 1)
				{
					buffer.Add(" and ");
				}
			}
		}
	}
}