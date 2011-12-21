using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using CK.Plugins.AutoClick.Views;

namespace CK.Plugins.AutoClick.Behaviors
{
    public class ComandOnMouseEnterBehavior : Behavior<ActionOnMouseEnterButton>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseEnter += ( sender, e ) =>
            {
                AssociatedObject.MouseEnterCommand.Execute( this );
            };
        }
    }
}