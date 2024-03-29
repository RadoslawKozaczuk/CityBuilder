﻿namespace Assets.GameLogic.Interfaces
{
    interface ICloneable<T>
    {
        /// <summary>
        /// Performs a shallow copy (value-types are copied entirely, reference-types only by reference).
        /// </summary>
        T Clone();
    }
}
