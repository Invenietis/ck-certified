using System;
using CK.Keyboard.Model;

namespace KeyboardEditor
{
    public interface IKeyboardBackupManager
    {
        /// <summary>
        /// This property holds the version of the keyboard that is being edited, before any modification.
        /// if the Filepath contained in this object is null while we are editing a keyboard, it means that this keyboard is a new one.
        /// </summary>
        KeyboardBackup KeyboardBackup { get; set; }

        /// <summary>
        /// Backs up a keyboard.
        /// Returns the file path where the keyboard has been backed up.
        /// 
        /// Throws a CKException if the IKeyboard implementation is not IStructuredSerializable
        /// </summary>
        /// <param name="keyboardToBackup">The keyboard ot backup</param>
        /// <returns>the path to the file in which the keyboard has been saved</returns>
        string BackupKeyboard( IKeyboard keyboardToBackup );

        /// <summary>
        /// Cancels all modifications made to the keyboard being currently modified
        /// Throws a <see cref="NullReferenceException"/> if <see cref="KeyboardBackup"/> is null.
        /// </summary>
        void CancelModifications();

        /// <summary>
        /// Deletes the backup file if it exists
        /// sets <see cref="KeyboardBackup"/> to null
        /// </summary>
        void EnsureBackupIsClean();
    }
}
