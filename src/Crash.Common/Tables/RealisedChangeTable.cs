using BidirectionalMap;

using Crash.Changes.Extensions;
using Crash.Common.Document;

using RhinoGuid = System.Guid;
using ChangeGuid = System.Guid;

namespace Crash.Common.Tables
{
	
	/// <summary>
	/// Performs change indexing
	/// </summary>
	public class RealisedChangeTable
	{
		public const string ChangeIdKey = "ChangeID";

		public readonly CrashDoc CrashDoc;

		private readonly BiMap<RhinoGuid, ChangeCache> RhinoChangeMap;

		public RealisedChangeTable(CrashDoc crashDoc)
		{
			CrashDoc = crashDoc;
			RhinoChangeMap = new BiMap<RhinoGuid, ChangeCache>();
		}

		public void AddPair(IChange change, RhinoGuid rhinoId)
		{
			if (ContainsRhinoId(rhinoId) || ContainsChange(change))
			{
				return;
			}

			RhinoChangeMap.Add(rhinoId, change);
		}

		public void UpdateChange(IChange change)
		{
		}

		public bool ContainsRhinoId(RhinoGuid rhinoId)
		{
			return RhinoChangeMap.Forward.ContainsKey(rhinoId);
		}

		public bool ContainsChange(IChange change)
		{
			return RhinoChangeMap.Reverse.ContainsKey(new ChangeCache(change));
		}

		public bool TryGetRhinoId(IChange change, out RhinoGuid rhinoId)
		{
			rhinoId = Guid.Empty;
			if (!ContainsChange(change))
			{
				PayloadPacket
				return false;
			}

			rhinoId = RhinoChangeMap.Reverse[new ChangeCache(change)];
			return true;
		}

		public bool TryGetChange(RhinoGuid rhinoId, out ChangeCache change)
		{
			change = default;
			if (!ContainsRhinoId(rhinoId))
			{
				return false;
			}

			change = RhinoChangeMap.Forward[rhinoId].;
			return true;
		}

		public bool UpdateChange(IChange change)
		{
			ChangeCache cache = new(change);
			if (!RhinoChangeMap.Reverse.ContainsKey(cache))
			{
				return false;
			}
		}

		internal record ChangeCache : IEquatable<ChangeCache>, IEquatable<Guid>
		{
			public ChangeCache(IChange change)
			{
				Id = change.Id;
				Update(change);
			}

			internal RhinoGuid RhinoId { get; private set; }
			internal ChangeGuid Id { get; }

			internal bool IsTemporary { get; private set; }

			internal bool IsSelected { get; private set; }

			public bool Equals(ChangeGuid changeId)
			{
				return Id == changeId;
			}

			internal bool Update(IChange change)
			{
				if (change.Id != Id)
				{
					return false;
				}

				IsTemporary = change.HasFlag(ChangeAction.Temporary);
				IsSelected = change.HasFlag(ChangeAction.Locked);

				return true;
			}

			public bool Equals(IChange change)
			{
				return change.Id == Id;
			}

			internal void SetRhinoObject(RhinoGuid rhinoId)
			{
				RhinoId = rhinoId;
			}

			public override int GetHashCode()
			{
				return Change.Id.GetHashCode();
			}
		}
	}
}
