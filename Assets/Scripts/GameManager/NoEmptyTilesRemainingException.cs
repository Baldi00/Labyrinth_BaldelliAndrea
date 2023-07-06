using System;

namespace DBGA.GameManager
{
    public class NoEmptyTilesRemainingException : Exception
    {
        public NoEmptyTilesRemainingException(string message) : base(message)
        {
        }
    }
}
