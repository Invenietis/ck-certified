using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;
using CK.WPF.ViewModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using CK.StandardPlugins.ObjectExplorer.ViewModels;

namespace CK.StandardPlugins.ObjectExplorer
{
    /// <summary>
    /// Basic class used to expose core properties. 
    /// It will be used by the <see cref="ObjectExplorer">Object Explorer plugin</see>.
    /// </summary>
    public abstract class VMIBase : VMBase, IDisposable
    {
        /// <summary>
        /// Gets a reference to the context.
        /// </summary>
        public VMIContextViewModel VMIContext { get; private set; }

        /// <summary>
        /// Gets the text that describe (shortly) the object.
        /// </summary>
        public string Label { get; protected set; }

        /// <summary>
        /// Gets the prefered path of the <see cref="VMIBase"/> into the treeview.
        /// </summary>
        public IList<VMIBase> Path { get { return null; } }

        /// <summary>
        /// Gets the <see cref="VMIBase">parent</see> of the <see cref="VMIBase"/>.
        /// </summary>
        public VMIBase Parent { get; private set; }
        
        /// <summary>
        /// Gets the name of the template that will be used 
        /// to display view model details.
        /// </summary>
        public string DetailsTemplateName { get; protected set; }

        public VMIBase( VMIContextViewModel context, VMIBase parent )
        {
            VMIContext = context;
            Parent = parent;
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}