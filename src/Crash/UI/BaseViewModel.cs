using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Crash.UI
{
	
	public abstract class BaseViewModel : INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
