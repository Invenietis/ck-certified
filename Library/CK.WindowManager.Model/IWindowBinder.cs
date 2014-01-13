using System;
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

        /// <summary>
        /// Does not bind the both <see cref="IWindowElement"/> provided, but provides a preview of a such <see cref="IBinding"/>.
        /// This will raised the event <see cref="PreviewBinding"/>.
        /// The returned <see cref="IBindResult"/> provides an interface to <see cref="IBindResult.Seal"/> or approve the previewed binding.
        /// </summary>
        /// <param name="target">This is the target window.</param>
        /// <param name="origin">This is the current window dragged by the user for example.</param>
        /// <param name="position">The position of the current relative to the target element. See <see cref="BindingPosition"/> remarks.</param>
        /// <returns><see cref="IBindResult"/></returns>
        IBindResult PreviewBind( IWindowElement target, IWindowElement origin, BindingPosition position );

        /// <summary>
        /// Does not unbind the both <see cref="IWindowElement"/> provided, but provides a preview of a such unbinding.
        /// This will raised the event <see cref="PreviewBinding"/>.
        /// The returned <see cref="IBindResult"/> provides an interface to <see cref="IBindResult.Seal"/> or approve the previewed unbinding.
        /// </summary>
        /// <param name="target">This is the target window.</param>
        /// <param name="origin">This is the current window dragged by the user for example.</param>
        /// <returns></returns>
        IBindResult PreviewUnbind( IWindowElement target, IWindowElement origin );

        /// <summary>
        /// Binds the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If an attachement already exists, this attachement is canceled. 
        /// The usage is to detach and the re-attach. Cannot replace alread attached window elements.
        /// No event is raised in such cases.
        /// </remarks>
        /// <param name="target">This is the target window.</param>
        /// <param name="origin">This is the current window dragged by the user for example.</param>
        /// <param name="position">The position of the current relative to the target element. See <see cref="BindingPosition"/> remarks.</param>
        void Bind( IWindowElement target, IWindowElement origin, BindingPosition position );

        /// <summary>
        /// Unbinds the both <see cref="IWindowElement"/> provided.
        /// </summary>
        /// <remarks>
        /// If no attachment exists between the both <see cref="IWindowElement"/> supplied, no event is raised.
        /// </remarks>
        /// <param name="target">This is the target window.</param>
        /// <param name="origin">This is the current window dragged by the user for example.</param>
        void Unbind( IWindowElement target, IWindowElement origin );

        /// <summary>
        /// Raised whan a preview of a binding occurs.
        /// </summary>
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
