using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace HaloInfiniteResearchTools.UI.Converters
{
    /// <summary>
     /// Provides a data template selector which honors data templates targeting interfaces implemented by the
     /// data context.
     /// </summary>
    public sealed class InterfaceTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement containerElement = container as FrameworkElement;

            if (null == item || null == containerElement)
                return base.SelectTemplate(item, container);

            Type itemType = item.GetType();

            IEnumerable<Type> dataTypes
                = Enumerable.Repeat(itemType, 1).Concat(itemType.GetInterfaces());

            DataTemplate template
                = dataTypes.Select(t => new DataTemplateKey(t))
                    .Select(containerElement.TryFindResource)
                    .OfType<DataTemplate>()
                    .FirstOrDefault();

            return template ?? base.SelectTemplate(item, container);
        }
    }
}
