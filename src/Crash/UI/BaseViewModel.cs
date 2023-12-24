using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crash.UI
{
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool Set<T>(ref T t, T val, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(t, val))
			{
				t = val;
				OnPropertyChanged(propertyName);
				return true;
			}

			return false;
		}
	}
}
