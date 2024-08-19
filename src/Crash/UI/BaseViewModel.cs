using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crash.UI
{

	public abstract class BaseViewModel : INotifyPropertyChanged
	{

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

	}

}
