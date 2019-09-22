namespace Assets.GameLogic.Interfaces
{
    interface ICommand
    {
        /// <summary>
        /// Returns true if command was already executed successfully and false otherwise.
        /// Successful call of Undo method will reset the flag and make this method return false again.
        /// </summary>
        bool Succeeded();

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
        /// Checks if this is valid context to execute this command. 
        /// For example, in case command applies to game map this method should check if mouse cursor is not over an UI element.
        /// </summary>
        bool CheckExecutionContext();

        /// <summary>
        /// This method assumes that command is in the valid execution context (CheckExecutionContext returns true).
        /// Checks if command's conditions are met. For example if player has enough resources, or if there is enough space for a building.
        /// </summary>
        bool CheckConditions();
    }
}