using Crash.Common.Document;
using Crash.Common.Tables;

namespace Crash.Handlers.Plugins.Layers
{
	internal class LayerTable : ICrashTable
	{
		internal LayerTable(CrashDoc crashDoc)
		{
			CrashDoc = crashDoc;
			Layers = new Dictionary<int, CrashLayer>();
		}

		private Dictionary<int, CrashLayer> Layers { get; }

		private CrashDoc CrashDoc { get; }

		internal void AddLayer(CrashLayer layer)
		{
			if (Layers.ContainsKey(layer.Index))
			{
				return;
			}

			Layers.Add(layer.Index, layer);
		}

		internal void MarkAsDeleted(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsDeleted = true;
			}
		}

		internal void RestoreFromDeleted(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsDeleted = false;
			}
		}

		internal void LockLayer(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsLocked = true;
			}
		}

		internal void UnlockLayer(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsDeleted = false;
			}
		}

		internal void HideLayer(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsVisible = true;
			}
		}

		internal void ShowLayer(int index)
		{
			if (Layers.TryGetValue(index, out var layer))
			{
				layer.IsVisible = true;
			}
		}

		internal void SetCurrent(int index)
		{
			foreach (var layer in Layers)
			{
				if (layer.Value.Index == index)
				{
					layer.Value.Current = true;
				}
				else
				{
					layer.Value.Current = false;
				}
			}
		}

		public void UpdateLayer(CrashLayer layer)
		{
			Layers.Remove(layer.Index);
			AddLayer(layer);
		}
	}
}
