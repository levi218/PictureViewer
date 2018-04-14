using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PictureViewer
{
    public class CoordinateConverter : IMultiValueConverter
    {
        public static double ThumbnailsWidth { get; set; }
        /* public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
         {
             //string[] sizes = ((String)parameter).Split('|');
             double ratio = ThumbnailsWidth / double.Parse((String)parameter);
             return double.Parse((String)value) * ratio;
         }
         */
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double ratio = ThumbnailsWidth / (Double)values[1];

            Console.WriteLine(" Thumbnail   "+ThumbnailsWidth +"  Ratio " +ratio+ "   Pos " +((Double)((Int32)values[0])) + "   Original Width " + (Double)values[1] );
            return Math.Round((Double)((Int32)values[0]) * ratio);
        }
        /*
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        */
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
