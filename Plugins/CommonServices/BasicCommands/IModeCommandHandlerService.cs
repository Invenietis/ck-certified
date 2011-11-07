using System;
using CK.Plugin;
using CK.Keyboard.Model;

namespace CommonServices
{
    /// <summary>
    /// Service which allow you to change the current keyboard mode and listen all mode changes done from this command handler.
    /// </summary>
    public interface IModeCommandHandlerService : IDynamicService
    {
        /// <summary>
        /// Changes the current mode of the current keyboard.
        /// </summary>
        /// <param name="mode">String which represent the new mode. It can be a composite mode.</param>
        void ChangeMode( string mode );

        /// <summary>
        /// Add the given mode to the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Add( string mode );

        /// <summary>
        /// Remove the given mode to the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Remove( string mode );

        /// <summary>
        /// Toogle the given mode with the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Toggle( string mode );

        /// <summary>
        /// Raised when the mode is changed by this service.
        /// </summary>
        event EventHandler<ModeChangedEventArgs> ModeChangedByCommandHandler;
    }

    public class ModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// String which represent the new mode.
        /// </summary>
        public readonly IKeyboardMode Mode;

        public ModeChangedEventArgs( IKeyboardMode mode ) { Mode = mode; }

    }
}