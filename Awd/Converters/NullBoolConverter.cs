using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Awd.Converters
{
	public class NullBoolConverter : IValueConverter
	{
		public bool Inverted { get; set; }
		public bool CreateDefaultInConvertBack { get; set; }

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value == null ? Inverted : !Inverted;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!CreateDefaultInConvertBack)
				return value;

			var b = (bool)value;
			if (!b)
				return null;

			else
			{
				if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					var type = targetType.GetGenericArguments();
					return Activator.CreateInstance(type.FirstOrDefault());

				}

				else
					return Activator.CreateInstance(targetType);
			}
		}
	}
}
