using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;

namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// This class is the viewmodel that corresponds to the "Profile" of a keyboard.
    /// Corresponds to its name, width and height.
    /// </summary>
    public class SimpleKeyboardViewModel
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Ctro to create a SimpleKeyboardViewModel from a <see cref="IKeyboard"/>
        /// </summary>
        /// <param name="keyboard">The model</param>
        public SimpleKeyboardViewModel( IKeyboard keyboard )
        {
            Name = keyboard.Name;
            Height = keyboard.CurrentLayout.H;
            Width = keyboard.CurrentLayout.W;
        }

        /// <summary>
        /// Creates a default SimpleKeyboardViewModel
        /// </summary>
        public SimpleKeyboardViewModel()
        {
            Name = "Nouveau clavier";
            Height = 400;
            Width = 800;
        }
    }
}
