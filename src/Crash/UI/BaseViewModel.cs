using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crash.UI
{

	public abstract class BaseViewModel : INotifyPropertyChanged
	{

		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

			if (_propertyChangedActions.TryGetValue(propertyName, out var actions))
			{
				foreach (var action in actions)
				{
					action?.Invoke();
				}
			}
		}

		protected bool Set<T>(ref T t, T val, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(t, val))
			{
				t = val;
				NotifyPropertyChanged(propertyName);
				return true;
			}

			return false;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public BaseViewModel()
		{
		}

		private Dictionary<string, List<Action>> _propertyChangedActions { get; } = new Dictionary<string, List<Action>>();

		public void ListenToProperty(string propertyName, Action action)
		{
			if (!_propertyChangedActions.TryGetValue(propertyName, out var actions))
			{
				_propertyChangedActions.Add(propertyName, new List<Action> { action });
			}
			else
			{
				actions.Add(action);
			}
		}

	}

}
