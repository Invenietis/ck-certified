using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// The window binder allows two <see cref="IWindwoElement"/> to bind together.
    /// </summary>
    public interface IWindowBinder : IDynamicService
    {
        /// <summary>
        /// Gets a readonly collection of attached <see cref="IWindowElement"/> from a referential <see cref="IWindowElement"/>.
        /// </summary>
        /// <param name="master">The <see cref="IWindowElement"/> from where to retrieve all attached <see cref="IWindowElement"/>.</param>
        /// <returns></returns>
        ICKReadOnlyCollection<IWindowElement> GetAttachedElements( IWindowElement master );

        /// <summary>
        /// Attach the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If an attachement already exists, this attachement is canceled. 
        /// The usage is to detach and the re-attach. Cannot replace alread attached window elements.
        /// No event is raised in such cases.
        /// </remarks>
        /// <param name="master">The master window element</param>
        /// <param name="slave"></param>
        /// <param name="position">The position of the attachement relatively between master and slave. See <see cref="BindingPosition"/> remarks.</param>
        void Attach( IWindowElement master, IWindowElement slave, BindingPosition position );

        /// <summary>
        /// Detach the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If no attachment exists between the both <see cref="IWindowElement"/> supplied, no event is raised.
        /// </remarks>
        /// <param name="master"></param>
        /// <param name="slave"></param>
        void Detach( IWindowElement master, IWindowElement slave );

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
