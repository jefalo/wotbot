using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankBot
{
    class NotVisibleOnScreenException : Exception
    {
        public NotVisibleOnScreenException()
        {
            Helper.LogDebug("NotVisibleOnScreenException created");
        }
    }
    class CannotAimException : Exception
    {
        public CannotAimException()
        {
            Helper.LogDebug("CannotAimException created");
        }
    }
    class EnemyBaseReadException : Exception
    {
        public EnemyBaseReadException()
        {
            Helper.LogException("EnemyBaseReadException");
        }
    }
    class CannotMoveForwardException : Exception
    {
        public CannotMoveForwardException()
        {
            Helper.LogException("CannotMoveForwardException");
        }
    }
    class StatusChangeException : Exception
    {
        public StatusChangeException()
        {
            Helper.LogException("StatusChangeException");
        }
    }
}
