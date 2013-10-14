using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    public interface IBindResult : CK.Core.IFluentInterface
    {
        void Seal();
    }

    /// <summary>
    /// The window binder allows two <see cref="IWindwoElement"/> to bind together.
    /// </summary>
    public interface IWindowBinder : IDynamicService
    {
        /// <summary>
        /// Gets a <see cref="ISpatialBinding"/> from a reference <see cref="IWindowElement"/>.
        /// </summary>
        /// <param name="reference">The <see cref="IWindowElement"/> from where to retrieve all attached <see cref="IWindowElement"/>.</param>
        /// <returns></returns>
        ISpatialBinding GetBinding( IWindowElement reference );


        IBindResult PreviewBind( IWindowElement master, IWindowElement slave, BindingPosition position );

        IBindResult PreviewUnbind( IWindowElement master, IWindowElement slave );

        /// <summary>
        /// Binds the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If an attachement already exists, this attachement is canceled. 
        /// The usage is to detach and the re-attach. Cannot replace alread attached window elements.
        /// No event is raised in such cases.
        /// </remarks>
        /// <param name="master">The master window element</param>
        /// <param name="slave"></param>
        /// <param name="position">The position of the attachement relatively between master and slave. See <see cref="BindingPosition"/> remarks.</param>
        void Bind( IWindowElement master, IWindowElement slave, BindingPosition position );

        /// <summary>
        /// Unbinds the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If no attachment exists between the both <see cref="IWindowElement"/> supplied, no event is raised.
        /// </remarks>
        /// <param name="master"></param>
        /// <param name="slave"></param>
        void Unbind( IWindowElement master, IWindowElement slave );

        event EventHandler<WindowBindedEventArgs> PreviewBinding;

        /// <summary>
        /// Raised before an attachment occurs. This event can be canceled.
        /// </summary>
        event EventHandler<WindowBindingEventArgs> BeforeBinding;

        /// <summary>
        /// Rasied after an attachment occurs.
        /// </summary>
        event EventHandler<WindowBindedEventArgs> AfterBinding;


    }

}
