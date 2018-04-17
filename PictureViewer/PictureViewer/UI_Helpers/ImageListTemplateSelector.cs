
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PictureViewer
{
    public class ImageListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TrainingTemplate { get; set; }
        public DataTemplate ShowingTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;
            FrameworkElement frameworkElement = container as FrameworkElement;
            if (frameworkElement != null) {
                if (item.GetType() == typeof(ImageModel))
                {
                    //ShowingTemplate = frameworkElement.FindResource("imageItemShowingTemplate") as DataTemplate;
                    return ShowingTemplate;
                }
                else
                {
                    //TrainingTemplate = frameworkElement.FindResource("imageItemTrainingTemplate") as DataTemplate;
                    return TrainingTemplate;
                }
            } else return null;
        }
    }
}
