namespace Assets.Scripts.Interfaces
{
    interface ICommand
    {
        /// <summary>
        /// Returns true if command was already executed successfully and false otherwise.
        /// </summary>
        bool IsSucceeded();

        /// <summary>
        /// Executes command. Subsequent calls make no effect.
        /// Returns true if command was executed successfully and false otherwise.
        /// </summary>
        bool Call();

        /// <summary>
        /// Undo command. Subsequent calls make no effect.
        /// Returns true if command was undone successfully and false otherwise.
        /// </summary>
        bool Undo();

        /// <summary>
        /// Checks if this is valid context to execute this command. For example if mouse cursor is above game map.
        /// </summary>
        bool CheckExecutionContext();

        /// <summary>
        /// Checks is command's conditions are met. For example if player has enough resources.
        /// </summary>
        bool CheckConditions();
    }
}